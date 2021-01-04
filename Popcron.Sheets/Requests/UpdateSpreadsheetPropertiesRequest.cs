using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateSpreadsheetPropertiesRequest
    {
        public SpreadsheetProperties properties;
        public string fields;
    }
}