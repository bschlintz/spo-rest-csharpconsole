using System;
using Microsoft.SharePoint.Client;
using OfficeDevPnP.Core.Utilities;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace csharp_spo_rest
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://tenant.sharepoint.com/sites/portal");
            var credentials = CredentialManager.GetSharePointOnlineCredential($"https://{uri.Host}");

            InvokeRestApiPost(uri, credentials);
        }

        private static void InvokeRestApiPost(Uri uri, SharePointOnlineCredentials credentials)
        {
            var authCookie = credentials.GetAuthenticationCookie(uri);
            string requestDigest = string.Empty;

            //POST: Request Digest
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer.SetCookies(uri, authCookie);

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsync($"{uri.AbsoluteUri}/_api/contextinfo", null).Result;

                    dynamic json = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                    requestDigest = json.FormDigestValue;
                }
            }

            //POST: Delete File
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer.SetCookies(uri, authCookie);

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("X-RequestDigest", requestDigest);
                    client.DefaultRequestHeaders.Add("X-HTTP-Method", "DELETE");
                    client.DefaultRequestHeaders.Add("IF-MATCH", "*");

                    var filePath = $"{uri.AbsolutePath}/shared documents/sample.docx";
                    var response = client.PostAsync($"{uri.AbsoluteUri}/_api/web/getfilebyserverrelativeurl('{filePath}')", null).Result;
                }
            }
        }
    }
}
