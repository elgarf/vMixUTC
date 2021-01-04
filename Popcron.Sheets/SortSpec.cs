using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class SortSpec
    {
        public int dimensionIndex;
        public string sortOrder;
    }
}