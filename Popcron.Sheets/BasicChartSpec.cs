using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BasicChartSpec
    {
        public string chartType;
        public string legendPosition;
        public BasicChartAxis[] axis;
        public BasicChartDomain[] domains;
        public BasicChartSeries[] series;
        public int headerCount;
        public bool threeDimensional;
        public bool interpolateNulls;
        public string stackedType;
        public bool lineSmoothing;
        public string compareMode;
    }
}