using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class ChartSpec
    {
        public string title;
        public string altText;
        public TextFormat titleTextFormat;
        public TextPosition titleTextPosition;
        public string subtitle;
        public TextFormat subtitleTextFormat;
        public TextPosition subtitlePosition;
        public string fontName;
        public bool maximized;
        public Color backgroundColor;
        public string hiddenDimensionStrategy;

        public BasicChartSpec basicChart;
        public PieChartSpec pieChart;
        public BubbleChartSpec bubbleChart;
        public CandlestickChartSpec candlestickChart;
        public OrgChartSpec orgChart;
        public HistogramChartSpec histogramChart;
        public WaterfallChartSpec waterfallChart;
        public TreemapChartSpec treemapChart;
    }
}