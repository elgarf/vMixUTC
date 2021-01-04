using System;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class SpreadsheetRaw
    {
        public string spreadsheetId;
        public SpreadsheetProperties properties;
        public SheetRaw[] sheets;
        public NamedRange[] namedRanges;
        public string spreadsheetUrl;
        public DeveloperMetadata[] developerMetadata;

        /// <summary>  
        /// Returns the raw data that is contingent to the Google Sheets API.  
        /// </summary>
        public static async Task<SpreadsheetRaw> Get(string spreadsheetId, Authorization authorization, bool includeGridData, SheetsSerializer serializer = null)
        {
            if (serializer == null) serializer = SheetsSerializer.Serializer;
            if (serializer == null) throw new Exception("No serializer was given.");

            SheetsClient client = new SheetsClient(spreadsheetId, authorization, serializer);
            SpreadsheetRaw raw = await client.GetRaw(includeGridData);

            return raw;
        }
    }
}