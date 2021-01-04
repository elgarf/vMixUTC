using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class Spreadsheet
    {
        /// <summary>  
        /// Returns all of the sheets inside this spreadsheet.  
        /// </summary> 
        public List<Sheet> Sheets
        {
            get
            {
                return sheets;
            }
        }

        /// <summary>  
        /// Returns the spreadsheetId.  
        /// </summary> 
        public string ID
        {
            get
            {
                return raw.spreadsheetId;
            }
        }

        /// <summary>  
        /// Returns the title of the spreadsheet.  
        /// </summary>
        public string Title
        {
            get
            {
                return raw.properties.title;
            }
        }

        /// <summary>  
        /// Returns the link the access this spreadsheet from a browser.  
        /// </summary>
        public string URL
        {
            get
            {
                return raw.spreadsheetUrl;
            }
        }

        private List<Sheet> sheets = new List<Sheet>();
        private SpreadsheetRaw raw;

        public Spreadsheet(SpreadsheetRaw raw)
        {
            this.raw = raw;

            for (int i = 0; i < raw.sheets.Length; i++)
            {
                Sheet sheet = new Sheet(raw.sheets[i]);
                sheets.Add(sheet);
            }
        }

        /// <summary>  
        /// Returns a high level representation of a spreadsheet.  
        /// </summary>
        public static async Task<Spreadsheet> Get(string spreadsheetId, Authorization authorization, SheetsSerializer serializer = null)
        {
            if (serializer == null) serializer = SheetsSerializer.Serializer;
            if (serializer == null) throw new Exception("No serializer was given.");

            SheetsClient client = new SheetsClient(spreadsheetId, authorization, serializer);
            Spreadsheet spreadsheet = await client.Get();

            return spreadsheet;
        }
    }
}