using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class UpdateSheetPropertiesRequest
    {
        public SheetProperties properties;
        public string fields;
    }
}