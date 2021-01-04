using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class SheetsClient
    {
        private readonly string spreadsheetId;
        private readonly Authorization authorization;
        private SheetsSerializer serializer;

        public SheetsClient(string spreadsheetId, Authorization authorization, SheetsSerializer serializer = null)
        {
            if (serializer == null) serializer = SheetsSerializer.Serializer;
            if (serializer == null) throw new Exception("No serializer was given.");

            this.serializer = serializer;
            this.spreadsheetId = spreadsheetId;
            this.authorization = authorization;
        }

        protected virtual T DeserializeObject<T>(string data)
        {
            return serializer.DeserializeObject<T>(data);
        }

        protected virtual string SerializeObject(object data)
        {
            return serializer.SerializeObject(data);
        }

        /// <summary>  
        /// Returns a high level representation of a spreadsheet.  
        /// </summary>
        public async Task<Spreadsheet> Get()
        {
            var raw = await GetRaw(true);
            Spreadsheet spreadsheet = new Spreadsheet(raw);
            return spreadsheet;
        }

        /// <summary>  
        /// Returns the raw data that is contingent to the Google Sheets API.  
        /// </summary>
        public async Task<SpreadsheetRaw> GetRaw(bool includeGridData)
        {
            string address = "https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}?{auth}&includeGridData=" + includeGridData.ToString().ToLower();
            address = address.Replace("{spreadsheetId}", spreadsheetId);

            string token = authorization.ToString();
            if (authorization.Type == AuthorizationType.Key)
            {
                address = address.Replace("{auth}", "key=" + token);
            }
            else if (authorization.Type == AuthorizationType.AccessToken)
            {
                address = address.Replace("{auth}", "accessToken=" + token);
            }

            using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                string data = await webClient.DownloadStringTaskAsync(address);
                SpreadsheetRaw spreadsheet = DeserializeObject<SpreadsheetRaw>(data);
                return spreadsheet;
            }
        }

        /// <summary>  
        /// Creates a spreadsheet, returning the newly created spreadsheet.  
        /// </summary>
        public async Task<SpreadsheetRaw> Create(SpreadsheetRaw spreadsheet)
        {
            string address = "https://sheets.googleapis.com/v4/spreadsheets?{auth}";

            string token = authorization.ToString();
            if (authorization.Type == AuthorizationType.Key)
            {
                address = address.Replace("{auth}", "key=" + token);
            }
            else if (authorization.Type == AuthorizationType.AccessToken)
            {
                address = address.Replace("{auth}", "accessToken=" + token);
            }

            string data = SerializeObject(spreadsheet);

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string response = await webClient.UploadStringTaskAsync(address, data);

                return DeserializeObject<SpreadsheetRaw>(response);
            }
        }

        /// <summary>  
        /// Applies one or more updates to the spreadsheet.  
        /// </summary>
        public async Task<RequestBatchUpdateResponse> BatchUpdate(RequestBatchUpdate request)
        {
            string address = "https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}:batchUpdate";
            address = address.Replace("{spreadsheetId}", spreadsheetId);

            string token = authorization.ToString();
            if (authorization.Type == AuthorizationType.Key)
            {
                token = "?key=" + token;
            }

            string data = SerializeObject(request);
            using (WebClient webClient = new WebClient())
            {
                if (authorization.Type == AuthorizationType.AccessToken)
                {
                    webClient.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;
                }

                webClient.Headers[HttpRequestHeader.Accept] = "application/json";
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                string response = await webClient.UploadStringTaskAsync(address, data);
                return DeserializeObject<RequestBatchUpdateResponse>(response);
            }
        }
    }
}