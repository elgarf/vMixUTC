using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class WaterfallChartSpecies
    {
        public ChartData data;
        public WaterfallChartColumnStyle positiveColumnsStyle;
        public WaterfallChartColumnStyle negativeColumnsStyle;
        public WaterfallChartColumnStyle subtotalColumnsStyle;
        public bool hideTrailingSubtotal;
        public WaterfallChartCustomSubtotal[] customSubtotals;
    }
}