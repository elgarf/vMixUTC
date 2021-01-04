using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SpreadsheetProperties
    {
        public string title;
        public string locale;
        public string autoRecalc;
        public string timeZone;
        public CellFormat defaultFormat;
        public IterativeCalculationSettings iterativeCalculationSettings;
    }
}
