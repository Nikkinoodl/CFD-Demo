﻿using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Diagnostics;

namespace CFD_Demo
{
    public partial class Form1 : Form
    {
        public PMCollection? pmCollection;
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
        /// Rewrite this to do the following:
        ///  More compact code
        ///  Timer
        ///  Numpy.Net or some other way to speed things up
        ///  Tighten up plotting
        /// </summary>
        /// <returns></returns>
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

            //Precompute cell sizes, squares and inverses
            var dx2 = dx * dx;
            var dy2 = dy * dy;
            var dxi = 1 / dx;
            var dyi = 1 / dy;
            var dxi2 = dxi * dxi;
            var dyi2 = dyi * dyi;
            var dxy2i = 1 / (dx2 + dy2);

            //Arrays for velocities and pressure
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

            //Timesteps
            for (float t = 0; t < tmax + dt; t += dt)
            {
                //u and v momentum calcs can be done in parallel
                Parallel.Invoke(() =>
                {
                    int i, j;
                    uStar = (double[,])u.Clone();

                    //u momentum 
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

                    vStar = (double[,])v.Clone();

                    //v momentum 
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
                double[,] pn = (double[,])p.Clone();

                //While optional iteration on p
                int n = 0;
                while (n < 1)
                {
                    for (j = 1; j < ny - 1; j++)
                    {
                        for (i = 1; i < nx - 1; i++)
                        {
                            p[i, j] = ((pn[i + 1, j] + pn[i - 1, j]) * dy2 + (pn[i, j + 1] + pn[i, j - 1]) * dx2 - b[i, j] * dx2 * dy2) * 0.5 * dxy2i;
                        }
                    }

                    n++;
                }

                //Reset pressure boundary conditions
                for (i = 0; i < nx; i++)
                {
                    //dp/dy = 0 at bottom (Neuman)
                    p[i, 0] = p[i, 1];
                }
                for (j = 0; j < ny; j++)
                {
                    //dp/dx = 0 at LHS and RHS (Neuman)
                    p[0, j] = p[1, j];
                    p[nx - 1, j] = p[nx - 2, j];
                }

                //Pressure correction with central differencing for dp
                for (j = 1; j < ny - 1; j++)
                {
                    for (i = 1; i < nx - 1; i++)
                    {
                        u[i, j] = uStar[i, j] - (p[i + 1, j] - p[i - 1, j]) * 0.5 * dxi * dt * rhoi;
                        v[i, j] = vStar[i, j] - (p[i, j + 1] - p[i, j - 1]) * 0.5 * dyi * dt * rhoi;
                    }
                }

                //***We do not need to reset u, v, p Dirichlet conditions at the boundary
            }

            //Display Reynolds number
            float reynolds = (float)(uTop * Lx / nu);
            if (nu > 0)
            {
                DisplayRe("Re : " + reynolds.ToString());
            };

            //Display CFL
            float CFL = (float)(uTop * dt * dxi);
            if (dx > 0)
            {
                DisplayCFL("CFL : " + CFL.ToString());
            }

            PMCollection pmCollection = new(nx, ny, dx, dy, x, y, u, p, v);

            return pmCollection;
        }

        private void DisplayRe(string txt)
        {
            label14.Text = txt;
        }
        private void DisplayCFL(string txt)
        {
            label5.Text = txt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Run the lid cavity CFD
            pmCollection = SolveLidCavity();

            //Display initial plot depending on selection
            ChangeDisplayType();
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
    }
}