using AccessAzureResources.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AccessAzureResources
{
    public class Common
    {
        public string GetAccessTokenForUser(string resource = "https://management.azure.com/.default", string tenantId = null, string userName = null, string Password = null)
        {
            string url = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId);
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("username", userName));
            parameters.Add(new KeyValuePair<string, string>("password", Password));
            parameters.Add(new KeyValuePair<string, string>("grant_type", "password"));
            parameters.Add(new KeyValuePair<string, string>("client_id", "1950a258-227b-4e31-a9cf-717495945fc2")); // Client ID for PowerShell client.

            string scope = resource;
            if (resource.Equals("https://management.azure.com/", StringComparison.OrdinalIgnoreCase))
            {
                scope = "https://management.azure.com/.default";
            }
            else if (resource.Equals("https://dev.azuresynapse.net", StringComparison.OrdinalIgnoreCase))
            {
                scope = "https://dev.azuresynapse.net/.default";
            }
            else if (resource.Equals("https://graph.microsoft.com/", StringComparison.OrdinalIgnoreCase))
            {
                scope = "https://graph.microsoft.com/.default";
            }

            parameters.Add(new KeyValuePair<string, string>("scope", scope));
            parameters.Add(new KeyValuePair<string, string>("response_type", "id_token"));
            var response = HttpHelper.SendFormUrlEncodedRequest(url, null, parameters);
            if (response.Result.StatusCode == HttpStatusCode.OK)
            {
                var jsonResult = JsonDocument.Parse(response.Result.RawResponse);
                return jsonResult?.RootElement.GetProperty("access_token").ToString();
            }
            else
            {
                throw new InvalidOperationException("Failed to obtain the jwt token");
            }
        }

        public string GetAccessTokenOnBehalfOfApplication(string resource, string tenantId, string servicePrincepalId, string servicePrincepalSecret)
        {
            try
            {
                string url = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId);
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                data.Add(new KeyValuePair<string, string>("scope", resource));
                data.Add(new KeyValuePair<string, string>("client_id", servicePrincepalId));
                data.Add(new KeyValuePair<string, string>("client_secret", servicePrincepalSecret));

                var response = HttpHelper.SendFormUrlEncodedRequest(url, null, data).ConfigureAwait(false).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonResult = JsonDocument.Parse(response.RawResponse);
                    return jsonResult?.RootElement.GetProperty("access_token").ToString();
                }

                throw new InvalidOperationException("Failed to obtain the access token.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
