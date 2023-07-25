using System.Diagnostics;
using OxyPlot.WindowsForms;

namespace CFD_Demo
{
    public partial class Form1 : Form
    {
        private PMCollection? pmCollection;
        private double reynolds;
        public Form1()
        {
            InitializeComponent();

            textBox1.Text = "0.001";
            textBox2.Text = "5.0";
            textBox3.Text = "5.0";

            textBox7.Text = ".1";
            textBox8.Text = "1.15";

            textBox9.Text = "2";
            textBox10.Text = "2";

            textBox11.Text = "81";
            textBox12.Text = "81";
        }

        /// <summary>
        /// The main CFD solver
        /// </summary>
        /// <returns>PMCollection</returns>
        private PMCollection SolveLidCavity()
        {
            //Number of grid nodes
            int nx = int.Parse(textBox11.Text);
            int ny = int.Parse(textBox12.Text);

            //Length of grid
            var Lx = double.Parse(textBox9.Text); ;
            var Ly = double.Parse(textBox10.Text); ;

            //Fluid properties
            var nu = double.Parse(textBox7.Text);
            var rho = double.Parse(textBox8.Text);
            var rhoi = 1 / rho;

            //Time domain
            float dt = float.Parse(textBox1.Text);
            float tmax = float.Parse(textBox2.Text);

            //Velocity boundary conditions
            var uTop = double.Parse(textBox3.Text);

            int i;
            int j;

            //x, y used for plotting
            var x = new double[nx];
            var y = new double[ny];

            //Set x, y position of grid nodes.
            for (i = 0; i < nx; i++)
            {
                x[i] = i * Lx / (nx - 1);
            }

            for (j = 0; j < ny; j++)
            {
                y[j] = j * Ly / (ny - 1);
            }

            //Cell size
            var dx = x[1] - x[0];
            var dy = y[1] - y[0];

            //Precompute cell size squares and inverses
            var dx2 = dx * dx;
            var dy2 = dy * dy;
            var dxi = 1 / dx;
            var dyi = 1 / dy;
            var dxi2 = dxi * dxi;
            var dyi2 = dyi * dyi;
            var dxy2i = 1 / (dx2 + dy2);

            //Arrays for velocities and pressure set to zero by default
            var u = new double[nx, ny];
            var v = new double[nx, ny];
            double[,] uStar = new double[nx, ny];
            double[,] vStar = new double[nx, ny];

            var b = new double[nx, ny];
            var p = new double[nx, ny];

            //Initial non-zero Dirichlet bc's
            //Everything else is initialized to zero by default
            for (i = 0; i < nx; i++)
            {
                //Top edge
                u[i, ny - 1] = uTop;
                p[i, ny - 1] = 1;             
            }

            //Overwrite top corner u's to zero
            u[0, ny - 1] = 0;
            u[nx - 1, ny - 1] = 0;

            //Timer for the main calc loop
            Stopwatch watch = new();
            watch.Start();

            //Timesteps
            for (float t = 0; t < tmax + dt; t += dt)
            {
                //u and v momentum calcs are done in parallel (but only saves < 1s on 81x81 grid)
                Parallel.Invoke(() =>
                {
                    int i, j;

                    //u momentum prediction step
                    for (j = 1; j < ny - 1; j++)   //bc's set u, v on boundary
                    {
                        for (i = 1; i < nx - 1; i++) //bc's set u, v on boundary
                        {
                            uStar[i, j] = u[i, j] +
                                dt * (nu * (u[i - 1, j] - 2 * u[i, j] + u[i + 1, j]) * dxi2 +
                                      nu * (u[i, j - 1] - 2 * u[i, j] + u[i, j + 1]) * dyi2 -
                                      0.5 * u[i, j] * (u[i + 1, j] - u[i - 1, j]) * dxi -
                                      0.5 * v[i, j] * (u[i, j + 1] - u[i, j - 1]) * dyi);
                        }
                    }
                },
                () =>
                {
                    int i, j;

                    //v momentum prediction step
                    for (j = 1; j < ny - 1; j++)
                    {
                        for (i = 1; i < nx - 1; i++)
                        {
                            vStar[i, j] = v[i, j] +
                                dt * (nu * (v[i - 1, j] - 2 * v[i, j] + v[i + 1, j]) * dxi2 +
                                      nu * (v[i, j - 1] - 2 * v[i, j] + v[i, j + 1]) * dyi2 -
                                      0.5 * u[i, j] * (v[i + 1, j] - v[i - 1, j]) * dxi -
                                      0.5 * v[i, j] * (v[i, j + 1] - v[i, j - 1]) * dyi);
                        }
                    }
                });

                //Note: Although it's possible to do so (because the solution converges as t increases),
                //doing the pressure steps in parallel with the momentum calcs does not reduce compute time
                //significantly (only about 0.1s)

                //RHS term of PPE
                //b = (rho/dt) * divergence_vel_star calculated with central differencing
                for (j = 1; j < ny - 1; j++)
                {
                    for (i = 1; i < nx - 1; i++)
                    {
                        b[i, j] = rho * ((uStar[i + 1, j] - uStar[i - 1, j]) * 0.5 * dxi + (vStar[i, j + 1] - vStar[i, j - 1]) * 0.5 * dyi) / dt;
                    }
                }

                //Calculate pressure
                //While optional iteration on p
                int n = 0;
                while (n < 1)
                {
                    //Clone pressure array for a clear distinction between pn and pn + 1
                    double[,] pn = (double[,])p.Clone();

                    //Calculate P
                    for (j = 1; j < ny - 1; j++)
                    {
                        for (i = 1; i < nx - 1; i++)
                        {
                            p[i, j] = ((pn[i + 1, j] + pn[i - 1, j]) * dy2 + (pn[i, j + 1] + pn[i, j - 1]) * dx2 - b[i, j] * dx2 * dy2) * 0.5 * dxy2i;
                        }
                    }

                    ////Reset pressure boundary conditions
                    ////Note that we do not need to reset u, v, p Dirichlet bc's
                    for (i = 0; i < nx; i++)
                    {
                        //dp/dy = 0 at bottom (Neumann)
                        p[i, 0] = p[i, 1];
                    }
                    for (j = 0; j < ny; j++)
                    {
                        //dp/dx = 0 at LHS and RHS (Neumann)
                        p[0, j] = p[1, j];
                        p[nx - 1, j] = p[nx - 2, j];
                    }

                    n++;
                }

                //Pressure correction step with central differencing for dp
                for (j = 1; j < ny - 1; j++)
                {
                    for (i = 1; i < nx - 1; i++)
                    {
                        u[i, j] = uStar[i, j] - (p[i + 1, j] - p[i - 1, j]) * 0.5 * dxi * dt * rhoi;
                        v[i, j] = vStar[i, j] - (p[i, j + 1] - p[i, j - 1]) * 0.5 * dyi * dt * rhoi;
                    }
                }
            }

            watch.Stop();
            DisplayTime(watch.Elapsed.ToString());

            //Calculate and display Reynolds number
            DisplayRe(nu, uTop, Lx);

            //Display CFL
            DisplayCFL(nu, uTop, dxi, dt);      

            PMCollection pmCollection = new(nx, ny, reynolds, uTop, dx, dy, x, y, u, v, p);

            return pmCollection;
        }

        private void DisplayRe(double nu, double uTop, double Lx)
        {
            //Calculate and display Reynolds number
            if (nu > 0)
                reynolds = Math.Round((uTop * Lx / nu), 2);
            else
                reynolds = 0;

            label14.Text = "Re: " + reynolds.ToString("0.00");
        }
        private void DisplayCFL(double nu, double uTop, double dxi, float dt)
        {
            double CFL = Math.Round((uTop * dt * dxi), 3);
            double vN = Math.Round((2* nu * dt * dxi * dxi), 3);

            label5.Text = "CFL/v Neumann: " + CFL.ToString("0.000") + " / " + vN.ToString("0.000");
        }

        private void DisplayTime(string txt)
        {
            label6.Text = "Time Elapsed : " + txt;
        }

        /// <summary>
        /// Calls the CFD solution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //Run the lid cavity CFD
            pmCollection = SolveLidCavity();

            //Display initial plot depending on selection
            ChangeDisplayType();
        }

        /// <summary>
        /// Saves the current plotted image to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            var plotModel = plot1.Model;

            SaveFileDialog f = new();
            f.AddExtension = true;
            f.Filter = "Portable Network Graphics|*.png";
            f.FileName = plotModel.Title + ".png";
            if (f.ShowDialog() == DialogResult.OK)
            {
                var pngExporter = new PngExporter { Width = 800, Height = 800, Resolution = 300 };
                pngExporter.ExportToFile(plotModel, f.FileName);
            }
        }

        /// <summary>
        /// Change display to show u velocity, v velocity or pressure p depending on which
        /// radio button is checked
        /// </summary>
        private void ChangeDisplayType()
        {
            if (radioButton1.Checked == true && pmCollection != null)
            {
                plot1.Model = pmCollection.uModel;
                plot1.Refresh();
            }
            if (radioButton2.Checked == true && pmCollection != null)
            {
                plot1.Model = pmCollection.vModel;
                plot1.Refresh();
            }
            if (radioButton3.Checked == true && pmCollection != null)
            {
                plot1.Model = pmCollection.pModel;
                plot1.Refresh();
            }
            if (radioButton4.Checked == true && pmCollection != null)
            {
                plot1.Model = pmCollection.lModel;
                plot1.Refresh();
            }
        }
        private void radioButton1_Click(object sender, EventArgs e)
        {
            ChangeDisplayType();
        }
        private void radioButton2_Click(object sender, EventArgs e)
        {
            ChangeDisplayType();
        }
        private void radioButton3_Click(object sender, EventArgs e)
        {
            ChangeDisplayType();
        }
        private void radioButton4_Click(object sender, EventArgs e)
        {
            ChangeDisplayType();
        }
    }
}