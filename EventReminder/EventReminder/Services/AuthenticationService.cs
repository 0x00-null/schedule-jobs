using System;
using System.Collections.Generic;
using System.Net.Http;
using EventReminder.Utils;
using Newtonsoft.Json.Linq;

namespace EventReminder.Services
{
    public class AuthenticationService
    {
        public static String Authenticate(string username, string password)
        {
            try
            {
                var userCredentials =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"username", username},
                    {"password", password},
                    {"client_id", "client"},
                    {"client_secret", "secret"},
                    {"grant_type", "password"}
                });
                var client = new HttpClient();
                var tokenUrl = "token-url".StringSetting();
                var message = client.PostAsync(tokenUrl, userCredentials);
                var result = message.Result.Content.ReadAsStringAsync().Result;
                var obj = JObject.Parse(result);
                var token = (string)obj["access_token"];
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception("Authentication Failed.",ex);
            }
            
        }
    }
}