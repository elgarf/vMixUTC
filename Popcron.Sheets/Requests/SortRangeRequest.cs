using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SortRangeRequest
    {
        public GridRange range;
        public SortSpec[] sortSpecs;
    }
}