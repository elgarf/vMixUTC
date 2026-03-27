using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class SheetsClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        private readonly string spreadsheetId;
        private readonly Authorization authorization;
        private readonly SheetsSerializer serializer;

        public SheetsClient(string spreadsheetId, Authorization authorization, SheetsSerializer serializer = null)
        {
            if (serializer == null)
            {
                serializer = SheetsSerializer.Serializer;
            }

            if (serializer == null)
            {
                throw new Exception("No serializer was given.");
            }

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

        public async Task<Spreadsheet> Get()
        {
            var raw = await GetRaw(true);
            Spreadsheet spreadsheet = new Spreadsheet(raw);
            return spreadsheet;
        }

        public async Task<SpreadsheetRaw> GetRaw(bool includeGridData)
        {
            string address = "https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}?{auth}&includeGridData=" + includeGridData.ToString().ToLowerInvariant();
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

            using (var request = new HttpRequestMessage(HttpMethod.Get, address))
            using (var response = await HttpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string data = await response.Content.ReadAsStringAsync();
                SpreadsheetRaw spreadsheet = DeserializeObject<SpreadsheetRaw>(data);
                return spreadsheet;
            }
        }

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

            using (var request = new HttpRequestMessage(HttpMethod.Post, address))
            {
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                using (var response = await HttpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    string responseText = await response.Content.ReadAsStringAsync();
                    return DeserializeObject<SpreadsheetRaw>(responseText);
                }
            }
        }

        public async Task<RequestBatchUpdateResponse> BatchUpdate(RequestBatchUpdate request)
        {
            string address = "https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}:batchUpdate";
            address = address.Replace("{spreadsheetId}", spreadsheetId);

            string token = authorization.ToString();
            if (authorization.Type == AuthorizationType.Key)
            {
                address += "?key=" + token;
            }

            string data = SerializeObject(request);
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, address))
            {
                if (authorization.Type == AuthorizationType.AccessToken)
                {
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpRequest.Content = new StringContent(data, Encoding.UTF8, "application/json");

                using (var response = await HttpClient.SendAsync(httpRequest))
                {
                    response.EnsureSuccessStatusCode();
                    string responseText = await response.Content.ReadAsStringAsync();
                    return DeserializeObject<RequestBatchUpdateResponse>(responseText);
                }
            }
        }
    }
}
