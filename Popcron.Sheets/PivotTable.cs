using System;
using System.Collections.Generic;

namespace Popcron.Sheets
{
    [Serializable]
    public class PivotTable
    {
        public GridRange source;
        public PivotTable[] rows;
        public PivotTable[] columns;
        public Dictionary<string, PivotFilterCriteria> criteria;
        public PivotValue[] values;
        public string valueLayout;
    }
}