using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class AppendCellsRequest
    {
        public int sheetId;
        public RowData[] rows;
        public string fields;
    }
}