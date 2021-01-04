using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateChartSpecRequest
    {
        public int chartId;
        public ChartSpec spec;
    }
}