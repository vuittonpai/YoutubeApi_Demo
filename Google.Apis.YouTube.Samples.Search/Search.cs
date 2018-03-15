/*
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Net;
using System.Text;
using System.Linq;
using System.Timers;


namespace Google.Apis.YouTube.Samples
{
  /// <summary>
  /// YouTube Data API v3 sample: search by keyword.
  /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
  /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
  ///
  /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
  ///   https://cloud.google.com/console
  /// Please ensure that you have enabled the YouTube Data API for your project.
  /// </summary>
  internal class Search
  {
        private static int _localFilename = 0;
        [STAThread]
        static void Main(string[] args)
        {
          Console.WriteLine("YouTube Data API: Search");
          Console.WriteLine("========================");

          try
          {
                var task2 = Task.Run(async () => {
                    while (true)
                    {
                        try
                        {
                            await new Search().RunEHS_Live();
                        }
                        catch (Exception e )
                        {
                            //super easy error handling
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                });

                Console.ReadKey();
                //new Search().RunEHS_Live().Wait();
          }
          catch (AggregateException ex)
          {
            foreach (var e in ex.InnerExceptions)
            {
              Console.WriteLine("Error: " + e.Message);
            }
          }

          Console.WriteLine("Press any key to continue...");
          Console.ReadKey();
        }

        private async Task Run()
        {
          var youtubeService = new YouTubeService(new BaseClientService.Initializer()
          {
            ApiKey = "AIzaSyCQSPPkxX5nyhpSWkjzvHwhDLkAkmovNuw",
            ApplicationName = this.GetType().ToString()
          });

          var searchListRequest = youtubeService.Search.List("snippet");
          //searchListRequest.Q = "Google"; // Replace with your search term.
          searchListRequest.MaxResults = 50;
          searchListRequest.Type = "video";
          searchListRequest.ChannelId = "UCbHepvlYSgOf507HzCT2Dsg";
          searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live;

          // Call the search.list method to retrieve results matching the specified query term.
          var searchListResponse = await searchListRequest.ExecuteAsync();

          List<string> videos = new List<string>();
          List<string> channels = new List<string>();
          List<string> playlists = new List<string>();

          // Add each result to the appropriate list, and then display the lists of
          // matching videos, channels, and playlists.
          foreach (var searchResult in searchListResponse.Items)
          {
            switch (searchResult.Id.Kind)
            {
              case "youtube#video":
                videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                break;

              case "youtube#channel":
                channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                break;

              case "youtube#playlist":
                playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                break;
            }
          }

          Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
          Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
          Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));
        }


        /// <summary>
        /// 這個範例是用來測試東森sensengo2015 這台的圖片位置變化規則 https://www.youtube.com/channel/UCiolqpxuocdomP4hPGfn_-A
        /// https://i.ytimg.com/vi/VYcQ3Xvjtzo/hqdefault_live.jpg?sqp=CKCp-NAF-oaymwEXCNACELwBSFryq4qpAwkIARUAAIhCGAE=&rs=AOn4CLBk8WBIS0fLutU4EseYAsdCK9pgxQ
        /// 產出的資料會每分鐘建立一個folder，每兩秒下載一次圖片
        /// LiveBroadCast Api 找不到有產出的sqp或是rs 的parameter，我猜被拿掉了
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task RunEHS_Live()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCjaGdZl66FK9jG7G3WGOFYa3FtlLwH5Kw",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = "EHS_Live"; // Replace with your search term.
            searchListRequest.MaxResults = 50;
            searchListRequest.Type = "video";
            searchListRequest.ChannelId = "UCiolqpxuocdomP4hPGfn_-A";
            searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        break;

                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        break;

                    case "youtube#playlist":
                        playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        break;
                }
            }

            var resutTest = searchListResponse?.Items?.Where(s => s.Id.VideoId == "VYcQ3Xvjtzo").FirstOrDefault();

            
            string path = "";
            using (WebClient client = new WebClient())
            {
                CreateIfMissing( DateTime.Now.ToString("yyyy_MM_dd_hh_mm"));
                client.DownloadFile($"https://i.ytimg.com/vi/VYcQ3Xvjtzo/sddefault_live.jpg?sqp={DateTime.Now.ToString("mm_ss_hh")}&rs={DateTime.Now.ToString("mm_ss_hh")}", $"F:\\localpath\\{DateTime.Now.ToString("yyyy_MM_dd_hh_mm")}\\EHS_Live_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.jpg");
            }

            

        }
        private void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory($"F:\\localpath\\{path}");
        }



    }
}
