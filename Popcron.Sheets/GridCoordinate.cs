using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class GridCoordinate
    {
        public int sheetId;
        public int rowIndex;
        public int columnIndex;
    }
}