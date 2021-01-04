using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DimensionRange
    {
        public int sheetId;
        public string dimension;
        public int startIndex;
        public int endIndex;
    }
}