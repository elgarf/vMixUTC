using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class RequestBatchUpdateResponse
    {
        public string spreadsheetId;
        public Response[] replies;
        public SpreadsheetRaw updatedSpreadsheet;
    }
}
