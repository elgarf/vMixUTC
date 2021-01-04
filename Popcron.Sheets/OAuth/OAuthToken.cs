using System;

namespace Popcron.Sheets
{
    [Serializable]
    public class OAuthToken
    {
        public string accessToken = "";
        public int expiresIn = 3600;
        public string refreshToken = "";
        public string scope = "";
        public string tokenType = "";
        public string idToken = "";
    }
}