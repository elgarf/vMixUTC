using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class TreemapChartSpec
    {
        public ChartData labels;
        public ChartData parentLabels;
        public ChartData sizeData;
        public ChartData colorData;
        public TextFormat textFormat;

        public int levels;
        public int hintedLevels;
        public double minValue;
        public double maxValue;
        public Color headerColor;
        public TreemapChartColorScale colorScale;
        public bool hideTooltips;
    }
}