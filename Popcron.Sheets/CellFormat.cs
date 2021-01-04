using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class CellFormat
    {
        public NumberFormat numberFormat;
        public Color backgroundColor;
        public Borders borders;
        public Padding padding;
        public string horizontalAlignment;
        public string verticalAlignment;
        public string wrapStrategy;
        public string textDirection;
        public TextFormat textFormat;
        public string hyperlinkDisplayType;
        public TextRotation textRotation;
    }
}