using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class InsertDimensionRequest
    {
        public DimensionRange range;
        public bool inheritFromBefore;
    }
}