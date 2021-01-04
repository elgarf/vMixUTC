using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DeleteRangeRequest
    {
        public GridRange range;
        public string shiftDimension;
    }
}