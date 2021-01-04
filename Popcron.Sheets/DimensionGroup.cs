using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DimensionGroup
    {
        public DimensionRange range;
        public int depth;
        public bool collapsed;
    }
}