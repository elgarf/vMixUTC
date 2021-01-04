using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BubbleChartSpec
    {
        public string legendPosition;
        public ChartData bubbleLabels;
        public ChartData domain;
        public ChartData series;
        public ChartData groupIds;
        public ChartData bubbleSizes;
        public float bubbleOpacity;
        public Color bubbleBorderColor;
        public uint bubbleMaxRadiusSize;
        public uint bubbleMinRadiusSize;
        public TextFormat bubbleTextStyle;
    }
}