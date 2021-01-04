using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BooleanRule
    {
        public BooleanCondition condition;
        public CellFormat format;
    }
}