using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class OrgChartSpec
    {
        public string nodeSize;
        public Color nodeColor;
        public Color selectedNodeColor;
        public ChartData labels;
        public ChartData parentLabels;
        public ChartData tooltips;
    }
}