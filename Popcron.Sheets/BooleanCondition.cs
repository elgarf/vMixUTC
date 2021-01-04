using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class BooleanCondition
    {
        public string type;
        public ConditionValue[] values;
    }
}