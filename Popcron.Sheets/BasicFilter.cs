using System;
using System.Collections.Generic;

namespace Popcron.Sheets
{
    [Serializable]
    public class BasicFilter
    {
        public GridRange range;
        public SortSpec[] sortSpecs;
        public Dictionary<string, FilterCriteria> criteria;
    }
}