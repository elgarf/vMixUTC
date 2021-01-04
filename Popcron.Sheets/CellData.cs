using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CellData
    {
        public ExtendedValue userEnteredValue;
        public ExtendedValue effectiveValue;
        public string formattedValue;
        public CellFormat userEnteredFormat;
        public CellFormat effectiveFormat;
        public string hyperlink;
        public string note;
        public TextFormatRun[] textFormatRuns;
        public DataValidationRule dataValidation;
        public PivotTable pivotTable;
    }
}