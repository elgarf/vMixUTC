using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class FilterCriteria
    {
        public string[] hiddenValues;
        public BooleanCondition condition;
    }
}