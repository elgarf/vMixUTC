using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        private static SheetsSerializer serializer;

        // ref http://stackoverflow.com/a/3978040
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

            // Generates state and PKCE values.
            string state = RandomDataBase64url(32);
            string codeVerifier = RandomDataBase64url(32);
            string codeChallenge = Base64urlencodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            //Console.WriteLine("Redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            HttpListener http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            //Console.WriteLine("Listening...");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                AuthorizationEndpoint,
                Uri.EscapeDataString(redirectURI),
                clientId,
                state,
                codeChallenge,
                codeChallengeMethod);

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Sends an HTTP response to the browser.
            HttpListenerResponse response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                //Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
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

            // extracts the code
            string code = context.Request.QueryString.Get("code");
            string incomingState = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incomingState != state)
            {
                string message = string.Format("Received request with invalid state ({0})", incomingState);
                throw new Exception(message);
            }

            //Console.WriteLine("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            return await PerformCodeExchange(code, codeVerifier, redirectURI, clientId, clientSecret);
        }

        private static async Task<OAuthToken> PerformCodeExchange(string code, string codeVerifier, string redirectURI, string clientId, string clientSecret)
        {
            //Console.WriteLine("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectURI),
                clientId,
                codeVerifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(byteVersion, 0, byteVersion.Length);
            stream.Close();

            // gets the response
            WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();
                //Console.WriteLine(responseText);

                // converts to dictionary
                Dictionary<string, string> tokenEndpointDecoded = serializer.DeserializeObject<Dictionary<string, string>>(responseText);

                OAuthToken token = new OAuthToken();
                token.accessToken = tokenEndpointDecoded["access_token"];
                token.expiresIn = int.Parse(tokenEndpointDecoded["expires_in"]);
                token.refreshToken = tokenEndpointDecoded["refresh_token"];
                token.scope = tokenEndpointDecoded["scope"];
                token.tokenType = tokenEndpointDecoded["token_type"];
                token.idToken = tokenEndpointDecoded["id_token"];

                return token;
                //UserinfoCall(access_token);
            }
        }

        //private static async void UserinfoCall(string access_token)
        //{
        //    Console.WriteLine("Making API Call to Userinfo...");

        //    // builds the  request
        //    string userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";

        //    // sends the request
        //    HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
        //    userinfoRequest.Method = "GET";
        //    userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
        //    userinfoRequest.ContentType = "application/x-www-form-urlencoded";
        //    userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        //    // gets the response
        //    WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
        //    using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
        //    {
        //        // reads response body
        //        string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
        //        Console.WriteLine(userinfoResponseText);
        //    }
        //}

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] Sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");

            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}