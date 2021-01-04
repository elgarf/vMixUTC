using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    [Serializable]
    public class Authorization
    {
        private string accessToken = null;
        private string key = null;

        public AuthorizationType Type
        {
            get
            {
                if (key != null) return AuthorizationType.Key;
                if (accessToken != null) return AuthorizationType.AccessToken;

                throw new Exception("Unknown Authorization state.");
            }
        }

        private Authorization()
        {

        }

        public override string ToString()
        {
            if (accessToken != null) return accessToken;
            if (key != null) return key;

            return null;
        }

        public static async Task<Authorization> Authorize(string clientId, string clientSecret, SheetsSerializer serializer = null)
        {
            if (serializer == null) serializer = SheetsSerializer.Serializer;
            if (serializer == null) throw new Exception("No serializer was given.");

            OAuthToken token = await OAuth.DoOAuth(clientId, clientSecret, serializer);

            Authorization authorization = new Authorization
            {
                accessToken = token.accessToken
            };

            return authorization;
        }

        public static async Task<Authorization> Authorize(string key, SheetsSerializer serializer = null)
        {
            if (serializer == null) serializer = SheetsSerializer.Serializer;
            if (serializer == null) throw new Exception("No serializer was given.");

            await Task.Delay(0);

            Authorization authorization = new Authorization
            {
                key = key
            };

            return authorization;
        }
    }
}
