using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class NamedRange
    {
        public string namedRangeId;
        public string name;
        public GridRange range;
    }
}