using Google.Apis.Auth.OAuth2;
using Google.Apis.Books.v1;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveBroadCastsDemo
{
    class Program
    {


        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Discovery API Sample");
            Console.WriteLine("====================");
            try
            {
                //ServiceAccount();

                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }



        /// <summary>
        /// 不用試了，官方已經說Service account 不支援 Youtube Api v3，
        /// 浪費我超多時間
        /// </summary>
        /// <returns></returns>
        private async Task Run()
        {
            Console.WriteLine("Plus API - Service Account");
            Console.WriteLine("==========================");

            String serviceAccountEmail = "sa-livebroadcast@livebroadcastsdemo.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"Key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { YouTubeService.Scope.YoutubeForceSsl }
               }.FromCertificate(certificate));

            // Create the service.
            YouTubeService youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
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

        }

        private async Task RunOAuth()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { BooksService.Scope.Books },
                    "user", CancellationToken.None, new FileDataStore("Books.ListMyLibrary"));
            }

            // Create the service.
            var service = new BooksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Books API Sample",
            });

            var bookshelves = await service.Mylibrary.Bookshelves.List().ExecuteAsync();

        }


        private static String ACTIVITY_ID = "z12gtjhq3qn2xxl2o224exwiqruvtda0i";
        private static void ServiceAccount()
        {
            Console.WriteLine("Plus API - Service Account");
            Console.WriteLine("==========================");

            String serviceAccountEmail = "SERVICE_ACCOUNT_EMAIL_HERE"; //大忌沒改到阿

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { PlusService.Scope.PlusMe }
               }.FromCertificate(certificate));

            // Create the service.
            var service = new PlusService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Plus API Sample",
            });







            Activity activity = service.Activities.Get(ACTIVITY_ID).Execute();
            Console.WriteLine("  Activity: " + activity.Object__.Content);
            Console.WriteLine("  Video: " + activity.Object__.Attachments[0].Url);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }




    }
}
