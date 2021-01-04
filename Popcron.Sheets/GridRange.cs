using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class GridRange
    {
        public int sheetId;
        public int startRowIndex;
        public int endRowIndex;
        public int startColumnIndex;
        public int endColumnIndex;
    }
}