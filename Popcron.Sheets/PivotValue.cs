using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class PivotValue
    {
        public string summarizeFunction;
        public string name;
        public string calculatedDisplayTable;
        public int sourceColumnOffset;
        public string formula;
    }
}