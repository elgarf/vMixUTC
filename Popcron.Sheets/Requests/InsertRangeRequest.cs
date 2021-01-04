using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class InsertRangeRequest
    {
        public GridRange range;
        public string shiftDimension;
    }
}