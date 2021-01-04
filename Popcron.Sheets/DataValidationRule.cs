using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DataValidationRule
    {
        public BooleanCondition condition;
        public string inputMessage;
        public bool strict;
        public bool showCustomUi;
    }
}