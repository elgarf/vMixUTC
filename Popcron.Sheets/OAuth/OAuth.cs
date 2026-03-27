using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Sheets
{
    public class OAuth
    {
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";

        private static readonly HttpClient HttpClient = new HttpClient();
        private static SheetsSerializer serializer;

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static async Task<OAuthToken> DoOAuth(string clientId, string clientSecret, SheetsSerializer serializer)
        {
            OAuth.serializer = serializer;

            string state = RandomDataBase64url(32);
            string codeVerifier = RandomDataBase64url(32);
            string codeChallenge = Base64urlencodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());

            HttpListener http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            http.Start();

            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                AuthorizationEndpoint,
                Uri.EscapeDataString(redirectURI),
                clientId,
                state,
                codeChallenge,
                codeChallengeMethod);

            System.Diagnostics.Process.Start(authorizationRequest);

            var context = await http.GetContextAsync();

            HttpListenerResponse response = context.Response;
            string responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            _ = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith(task =>
            {
                responseOutput.Close();
                http.Stop();
            });

            if (context.Request.QueryString.Get("error") != null)
            {
                string message = string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error"));
                throw new Exception(message);
            }

            if (context.Request.QueryString.Get("code") == null || context.Request.QueryString.Get("state") == null)
            {
                string message = "Malformed authorization response. " + context.Request.QueryString;
                throw new Exception(message);
            }

            string code = context.Request.QueryString.Get("code");
            string incomingState = context.Request.QueryString.Get("state");

            if (incomingState != state)
            {
                string message = string.Format("Received request with invalid state ({0})", incomingState);
                throw new Exception(message);
            }

            return await PerformCodeExchange(code, codeVerifier, redirectURI, clientId, clientSecret);
        }

        private static async Task<OAuthToken> PerformCodeExchange(string code, string codeVerifier, string redirectURI, string clientId, string clientSecret)
        {
            var tokenRequestBody = new Dictionary<string, string>
            {
                ["code"] = code,
                ["redirect_uri"] = redirectURI,
                ["client_id"] = clientId,
                ["code_verifier"] = codeVerifier,
                ["client_secret"] = clientSecret,
                ["scope"] = string.Empty,
                ["grant_type"] = "authorization_code"
            };

            using (var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint))
            {
                request.Content = new FormUrlEncodedContent(tokenRequestBody);
                using (var response = await HttpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    string responseText = await response.Content.ReadAsStringAsync();
                    Dictionary<string, string> tokenEndpointDecoded = serializer.DeserializeObject<Dictionary<string, string>>(responseText);

                    OAuthToken token = new OAuthToken
                    {
                        accessToken = tokenEndpointDecoded["access_token"],
                        expiresIn = int.Parse(tokenEndpointDecoded["expires_in"]),
                        refreshToken = tokenEndpointDecoded["refresh_token"],
                        scope = tokenEndpointDecoded["scope"],
                        tokenType = tokenEndpointDecoded["token_type"],
                        idToken = tokenEndpointDecoded["id_token"]
                    };

                    return token;
                }
            }
        }

        public static string RandomDataBase64url(uint length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return Base64urlencodeNoPadding(bytes);
        }

        public static byte[] Sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            return SHA256.HashData(bytes);
        }

        public static string Base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }
    }
}
