using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class WaterfallChartCustomSubtotal
    {
        public int subtotalIndex;
        public string label;
        public bool dataIsSubtotal;
    }
}