using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class AddConditionalFormatRuleRequest
    {
        public ConditionalFormatRule rule;
        public int index;
    }
}