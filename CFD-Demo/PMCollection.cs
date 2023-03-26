using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CFD_Demo
{
    public class PMCollection
    {
        public PlotModel uModel;
        public PlotModel vModel;
        public PlotModel pModel;
        public PlotModel lModel;

        public PMCollection(int nx, int ny, float re, double uTop, double dx, double dy, double[] x, double[] y, double[,] u, double[,] v, double[,] p)
        {

            //u, v and p plots are all based on heatmap and contours, so they can be set up together

            //Add heatmaps
            HeatMapSeries heatMap1 = new() { Data = u };
            HeatMapSeries heatMap2 = new() { Data = v };
            HeatMapSeries heatMap3 = new() { Data = p };

            List<HeatMapSeries> hmList = new() { heatMap1, heatMap2, heatMap3 };

            foreach (var h in hmList)
            {
                h.X0 = 0;
                h.Y0 = 0;
                h.X1 = (nx - 1) * dx;
                h.Y1 = (ny - 1) * dy;
                h.CoordinateDefinition = HeatMapCoordinateDefinition.Edge;
                h.Interpolate = true;
                h.Font = "Segoe UI";
                h.FontSize = 0.2;
                h.TextColor = OxyColors.Black;
                h.FontWeight = FontWeights.Normal;
                h.Background = OxyColors.White;
            }

            //Add contours
            ContourSeries cs1 = new() { Data = u };
            ContourSeries cs2 = new() { Data = v };
            ContourSeries cs3 = new() { Data = p };

            List<ContourSeries> csList = new() { cs1, cs2, cs3 };

            foreach (var c in csList)
            {
                c.Color = OxyColors.White;
                c.LineStyle = LineStyle.Solid;
                c.FontSize = 0;
                c.ContourLevelStep = 0.25;
                c.LabelBackground = OxyColors.Undefined;
                c.ColumnCoordinates = x;
                c.RowCoordinates = y;
                c.Background = OxyColors.White;
            }

            //Add plot models for u, v velocities and pressure p
            PlotModel _uModel = new() { Title = "U Velocities in Lid Cavity - Re " + re, Background = OxyColors.White };
            PlotModel _vModel = new() { Title = "V Velocities in Lid Cavity - Re " + re, Background = OxyColors.White };
            PlotModel _pModel = new() { Title = "Pressure P in Lid Cavity - Re " + re, Background = OxyColors.White };

            //Add axes and assign heat map and contour series
            List<PlotModel> pmList = new() { _uModel, _vModel, _pModel };

            int n = 0;

            foreach (var m in pmList)
            {
                m.Axes.Add(new LinearColorAxis
                {
                    Position = AxisPosition.Right,
                    Palette = OxyPalettes.Jet(256),
                    HighColor = OxyColors.Yellow,
                    LowColor = OxyColors.Aqua,
                    UseSuperExponentialFormat = true,
                    TextColor = OxyColors.Black
                });

                m.Axes.Add(new LinearAxis()
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    Position = AxisPosition.Left,
                    Title = "Y position"                    
                });

                m.Axes.Add(new LinearAxis()
                {
                    MajorGridlineStyle = LineStyle.Solid,
                    Position = AxisPosition.Bottom,
                    Title = "X Position"
                });

                m.Series.Add(hmList[n]);
                m.Series.Add(csList[n]);

                n++;
            }

            uModel = _uModel;
            vModel = _vModel;
            pModel = _pModel;

            //Midpoint line plots are created using a different series
            LineSeries uLine = new()
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5,
                Background = OxyColors.White
            };

            //Test for odd or even nx

            int midX;

            if (nx % 2 != 0)
                midX = (nx - 1) / 2;
            else
                midX = nx / 2;

            //Find U data points at the X center line
            for (int j = 0; j < ny; j++)
            {
                uLine.Points.Add(new DataPoint(y[j]/(dy * (ny - 1)), u[midX, j] / uTop));

            }

            //Add plot model for u velocities at midline
            PlotModel _lModel = new()
            {
                Title = "U Velocities at Mid Line - Re " + re,
                PlotType = PlotType.XY,
                Background = OxyColors.White
            };

            _lModel.Axes.Add(new LinearAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom,
                Title = "Height y/Ly" 
            }) ;

            _lModel.Axes.Add(new LinearAxis()
            {
                MajorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Left,
                Title = "u/U"
            });

            _lModel.Series.Add(uLine);

            lModel = _lModel;

        }
    }
}
