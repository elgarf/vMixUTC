using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class RepeatCellRequest
    {
        public GridRange range;
        public CellData cell;
        public string fields;
    }
}