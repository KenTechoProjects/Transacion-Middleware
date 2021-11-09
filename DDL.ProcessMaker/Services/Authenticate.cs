using DDL.ProcessMaker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DDL.ProcessMaker.Services
{
    public class Authenticate
    {
        static HttpClient client;

        private string _baseURL;
        private string _clientId;
        private string _clientSecret; 
        private string _tokenFileName;
        /// <summary>
        /// Helper class to connect to the ProcessMaker REST API
        /// </summary>
        /// <param name="baseURL">URL to the processmaker server with a trailing slash</param>
        /// <param name="clientID">Client ID of application generated on ProcessMaker</param>
        /// <param name="clientSecret">Client secret of application generated on ProcessMaker</param> 
        /// <param name="tokenFileName">(Optional) Filename of the stored access token</param>
        public Authenticate(string baseURL, string clientID, string clientSecret,
             string tokenFileName = "ouathAccess.json")
        {
            _baseURL = baseURL;
            _clientId = clientID;
            _clientSecret = clientSecret; 
            _tokenFileName = tokenFileName;
            HttpClientHandler clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            client = new HttpClient(clientHandler);
        }

        /// <summary>
        /// Log in to the ProcessMaker REST API to get an access token and save response to file
        /// </summary>
        /// <param name="username">Username of user to login with</param>
        /// <param name="password">Password of the user</param>
        private async Task<string> Login(string username, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    GrantType = "password",
                    Scope = "*",
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Username = username,
                    Password = password
                };

                var url = _baseURL + "workflow/oauth2/token";
                var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                var responseString = await response.Content.ReadAsStringAsync();

                //if is successful response
                if (responseString.StartsWith("{\"access_token\""))
                {
                    //save response to file
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseString);
                    var newResponseString = "";
                    if (tokenResponse.ExpiresIn > 23 * 60)
                    {
                        tokenResponse.ExpiresAt = DateTime.Now.AddMinutes(23);
                    }
                    else
                    {
                        tokenResponse.ExpiresAt = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                    }
                    newResponseString = JsonConvert.SerializeObject(tokenResponse);

                    string currentDirectory = Directory.GetCurrentDirectory();
                    string fullPath = Path.Combine(currentDirectory, _tokenFileName);
                    await File.WriteAllTextAsync(fullPath, newResponseString);
                    return tokenResponse.AccessToken;
                }
                else
                {
                    throw new HttpRequestException(responseString);
                }
            }
            catch (HttpRequestException e)
            {
                //TODO: Log exception
                throw e;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Get Access Token from ProcessMaker API or from store    
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<string> GetTokenCode(string username, string password)
        { 
            string currentDirectory = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(currentDirectory, _tokenFileName);
            if (File.Exists(fullPath))
            {
                var fileContent = await File.ReadAllTextAsync(fullPath);
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(fileContent);
                if (Convert.ToDateTime(tokenResponse.ExpiresAt) < DateTime.Now)
                {
                    return  await Login(username: username, password: password);
                }
                else
                {
                    return tokenResponse.AccessToken;
                }
            }
            else
            {
                return await Login(username: username, password: password);
            }
        }
    }
}
