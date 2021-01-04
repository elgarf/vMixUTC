using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class HistogramChartSpec
    {
        public HistogramSeries[] series;
        public string legendPosition;
        public bool showItemDividers;
        public double bucketSize;
        public double outlierPercentile;
    }
}