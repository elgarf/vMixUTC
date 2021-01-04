using System;
using System.Collections.Generic;

namespace Popcron.Sheets
{
    [Serializable]
    public class FilterView
    {
        public int filterViewId;
        public string title;
        public GridRange range;
        public string namedRangeId;
        public SortSpec[] sortSpecs;
        public Dictionary<string, FilterCriteria> criteria;
    }
}