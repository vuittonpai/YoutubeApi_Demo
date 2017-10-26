using Google.Apis.Analytics.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static Google.Apis.YouTube.v3.LiveBroadcastsResource.ListRequest;

namespace Google.Apis.YouTubeKeyP12.LiveBroadCast
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Program().ServiceAccount1().Wait();

        }


        /// <summary>
        /// 失敗，YouTubeService 不支援service account
        /// </summary>
        /// <returns></returns>
        private async Task ServiceAccount1()
        {
            string _exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");

            // Get active credential
            string credPath = _exePath + @"\client.json";

            var json = File.ReadAllText(credPath);
            var cr = JsonConvert.DeserializeObject<PersonalServiceAccountCred>(json); // "personal" service account credential

            // Create an explicit ServiceAccountCredential credential
            var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(cr.client_email)
            {
                Scopes = new[] {
                    YouTubeService.Scope.YoutubeForceSsl
                }
            }.FromPrivateKey(cr.private_key));

            // Create the service
            YouTubeService youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = xCred,
                    ApplicationName = this.GetType().ToString()
                }
            );




            var liveBroadcastListRequest = youtubeService.LiveBroadcasts.List("snippet,contentDetails,status");
            liveBroadcastListRequest.BroadcastStatus = LiveBroadcastsResource.ListRequest.BroadcastStatusEnum.Active;
            liveBroadcastListRequest.BroadcastType = LiveBroadcastsResource.ListRequest.BroadcastTypeEnum.All;
            liveBroadcastListRequest.MaxResults = 10;
            var liveBroadcastListResponse = await liveBroadcastListRequest.ExecuteAsync();


            if (liveBroadcastListResponse.Items.Count > 0)
            {
                foreach (var channel in liveBroadcastListResponse.Items)
                {
                    Console.WriteLine($"URL : https://www.youtube.com/watch?v={channel.Id}/");
                    Console.WriteLine($"URL : Http://www.youtube.com/channel/{channel.Snippet.ChannelId}/live");
                }

            }



            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        //失敗，YouTubeService 不支援service account
        private async Task ServiceAccount()
        {
            Console.WriteLine("Plus API - Service Account");
            Console.WriteLine("==========================");

            String serviceAccountEmail = "paiadmin@paiconsolelivetv.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);


           

            //source: https://zavitax.wordpress.com/2012/12/17/logging-in-with-google-service-account-in-c-jwt/

            // header
            var header = new { typ = "JWT", alg = "RS256" };

            // claimset
            var times = GetExpiryAndIssueDate();
            var claimset = new
            {
                iss = "paiadmin@paiconsolelivetv.iam.gserviceaccount.com",
                scope = YouTubeService.Scope.Youtube,
                aud = "https://accounts.google.com/o/oauth2/token",
                iat = times[0],
                exp = times[1],
            };

            JavaScriptSerializer ser = new JavaScriptSerializer();

            // encoded header
            var headerSerialized = ser.Serialize(header);
            var headerBytes = Encoding.UTF8.GetBytes(headerSerialized);
            var headerEncoded = Convert.ToBase64String(headerBytes);

            // encoded claimset
            var claimsetSerialized = ser.Serialize(claimset);
            var claimsetBytes = Encoding.UTF8.GetBytes(claimsetSerialized);
            var claimsetEncoded = Convert.ToBase64String(claimsetBytes);

            // input
            var input = headerEncoded + "." + claimsetEncoded;
            var inputBytes = Encoding.UTF8.GetBytes(input);

            // signiture
            var rsa = certificate.PrivateKey as RSACryptoServiceProvider;
            var cspParam = new CspParameters
            {
                KeyContainerName = rsa.CspKeyContainerInfo.KeyContainerName,
                KeyNumber = rsa.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2
            };
            var aescsp = new RSACryptoServiceProvider(cspParam) { PersistKeyInCsp = false };
            var signatureBytes = aescsp.SignData(inputBytes, "SHA256");
            var signatureEncoded = Convert.ToBase64String(signatureBytes);

            // jwt
            var jwt = headerEncoded + "." + claimsetEncoded + "." + signatureEncoded;

            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            var uri = "https://accounts.google.com/o/oauth2/token";
            var content = new NameValueCollection();

            content["assertion"] = jwt;
            content["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer";

            string response = Encoding.UTF8.GetString(client.UploadValues(uri, "POST", content));

            var result = ser.Deserialize<dynamic>(response);

     















            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { YouTubeService.Scope.Youtube }
               }.FromCertificate(certificate));

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "paiLivevideo",
            });

                        
            var livebroadList = service.LiveBroadcasts.List("id");
            livebroadList.BroadcastStatus = BroadcastStatusEnum.Active;
            livebroadList.BroadcastType = BroadcastTypeEnum.All;
            var ListBroadResponse = await livebroadList.ExecuteAsync();


           


            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        private static int[] GetExpiryAndIssueDate()
        {
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.UtcNow;

            var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
            var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;

            return new[] { iat, exp };
        }

        //失敗，因為我沒開GA service
        private async Task ServiceAccount2()
        {

            string _exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");

            // Get active credential
            string credPath = _exePath + @"\client.json";

            var json = File.ReadAllText(credPath);
            var cr = JsonConvert.DeserializeObject<PersonalServiceAccountCred>(json); // "personal" service account credential

            // Create an explicit ServiceAccountCredential credential
            var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(cr.client_email)
            {
                Scopes = new[] {
            AnalyticsService.Scope.AnalyticsManageUsersReadonly,
            AnalyticsService.Scope.AnalyticsReadonly
        }
            }.FromPrivateKey(cr.private_key));

            // Create the service
            AnalyticsService service = new AnalyticsService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = xCred,
                }
            );

            // some calls to Google API
            var act1 = service.Management.Accounts.List().Execute();

            var actSum = service.Management.AccountSummaries.List().Execute();

            var resp1 = service.Management.Profiles.List(actSum.Items[0].Id, actSum.Items[0].WebProperties[0].Id).Execute();
        }


    }



    public class PersonalServiceAccountCred
    {
        public string type { get; set; }
        public string project_id { get; set; }
        public string private_key_id { get; set; }
        public string private_key { get; set; }
        public string client_email { get; set; }
        public string client_id { get; set; }
        public string auth_uri { get; set; }
        public string token_uri { get; set; }
        public string auth_provider_x509_cert_url { get; set; }
        public string client_x509_cert_url { get; set; }
    }

}
