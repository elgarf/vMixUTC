using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateConditionalFormatRuleRequest
    {
        public int index;
        public int sheetId;
        public object instruction;

        public void Set(ConditionalFormatRule rule)
        {
            instruction = rule;
        }

        public void Set(int newIndex)
        {
            instruction = newIndex;
        }
    }
}