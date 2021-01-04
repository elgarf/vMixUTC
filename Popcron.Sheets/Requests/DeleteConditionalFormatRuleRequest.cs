using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class DeleteConditionalFormatRuleRequest
    {
        public int index;
        public int sheetId;
    }
}