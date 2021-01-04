using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class MergeCellsRequest
    {
        public GridRange range;
        public string mergeType;
    }
}