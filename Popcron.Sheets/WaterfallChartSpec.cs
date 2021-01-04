using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class WaterfallChartSpec
    {
        public WaterfallChartDomain domain;
        public WaterfallChartSpecies[] series;
        public string stackedType;
        public bool firstValueIsTotal;
        public bool hideConnectorLines;
        public LineStyle connectorLineStyle;
    }
}