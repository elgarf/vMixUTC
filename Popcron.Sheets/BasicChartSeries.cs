using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BasicChartSeries
    {
        public ChartData series;
        public string targetAxis;
        public string type;
        public LineStyle lineStyle;
        public Color color;
    }
}