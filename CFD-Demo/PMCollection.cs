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

        public PMCollection(int nx, int ny, double dx, double dy, double[] x, double[] y, double[,] u, double[,] v, double[,] p)
        {
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
                c.ContourLevelStep = 1;
                c.LabelBackground = OxyColors.Undefined;
                c.ColumnCoordinates = x;
                c.RowCoordinates = y;
            }

            //Add plot models for u, v velocities and pressure p
            PlotModel _uModel = new() { Title = "U Velocities in Lid Cavity" };
            PlotModel _vModel = new() { Title = "V Velocities in Lid Cavity" };
            PlotModel _pModel = new() { Title = "Pressure P in Lid Cavity" };

            //Add axes and assign heat map and contour series
            List<PlotModel> pmList = new List<PlotModel> { _uModel, _vModel, _pModel };

            int i = 0;

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

                m.Series.Add(hmList[i]);
                m.Series.Add(csList[i]);

                i++;
            }

            uModel = _uModel;
            vModel = _vModel;
            pModel = _pModel;

        }
    }
}
