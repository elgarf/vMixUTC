using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateConditionalFormatRuleResponse
    {
        public ConditionalFormatRule newRule;
        public int newIndex;

        public object old_info;
    }
}
