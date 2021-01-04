using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SheetProperties
    {
        public int sheetId;
        public string title;
        public int index;
        public string sheetType;
        public GridProperties gridProperties;
        public bool hidden;
        public Color tabColor;
        public bool rightToLeft;
    }
}