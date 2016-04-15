using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;
using Newtonsoft.Json;

namespace Cloudoh.Common
{
    public class SkydriveHelper
    {

        private static readonly string[] scopes = new[] { 
                "wl.signin", 
                "wl.skydrive",
                "wl.skydrive_update", 
                "wl.offline_access"
                };

        private LiveAuthClient authClient;
        private LiveConnectClient liveClient;

        private const string CloudohPlaylist = "cloudoh.playlists";

        protected string CloudohUploadFolder
        {
            get { return "Cloudoh"; }
        }

        private const string ClientId = "000000004010B717";
        //private const string Clientsecret = "0dTo74ICbldqjyqB1t7bhtOcUTvThSdB";

        private async Task<string> LoginAndGetFolderId()
        {
            authClient = new LiveAuthClient(ClientId);
            var result = await this.authClient.InitializeAsync(scopes);

            LiveConnectSession session;

            if (result.Status != LiveConnectSessionStatus.Connected)
            {
                var login = await authClient.LoginAsync(scopes);
                session = login.Session;
            }
            else
            {
                session = result.Session;
            }

            liveClient = new LiveConnectClient(session);

            var res = await liveClient.GetAsync("me/skydrive/files?filter=folders");

            // exists?    
            string folderId = null;

            dynamic results = JsonConvert.DeserializeObject(res.RawResult);

            foreach (var folder in results["data"])
            {
                if (folder.name == CloudohUploadFolder)
                {
                    folderId = folder.id;
                    break;
                }
            }

            if (folderId == null)
            {
                // create the folder
                // doesnt exist    
                var folderData = new Dictionary<string, object>
                                     {
                                         {"name", CloudohUploadFolder},
                                         {"description", "Folder used by Cloudoh to backup playlists and other settings to."}
                                     };

                var postResult = await liveClient.PostAsync("me/skydrive", folderData);

                folderId = postResult.Result["id"] as string;
            }

            return folderId;

        }


        public async Task UploadPlaylists(string contents)
        {

            string folderId = await LoginAndGetFolderId();

            // the extra null is to work around a bug where the overwrite doesnt work
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));

            var uploadResult = await liveClient.UploadAsync(folderId, CloudohPlaylist, memoryStream, OverwriteOption.Overwrite);                
            
        }

        public async Task<string> DownloadPlaylists()
        {

            string folderId = await LoginAndGetFolderId();

            var result = await liveClient.GetAsync(folderId + "/files");

            dynamic results = JsonConvert.DeserializeObject(result.RawResult);

            string fileId = "";

            foreach (var folder in results["data"])
            {
                if (folder.name == CloudohPlaylist)
                {
                    fileId = folder.id;
                    break;
                }
            }

            if (string.IsNullOrEmpty(fileId))
                return string.Empty;

            var resultStram = await liveClient.DownloadAsync(fileId + "/content");

            string contents;

            using (var reader = new StreamReader(resultStram.Stream, Encoding.UTF8))
            {
                contents = reader.ReadToEnd();
            }

            return contents;

        }


        public async Task<string> SharePlaylist(string contents, string playlistId)
        {

            string folderId = await LoginAndGetFolderId();

            // the extra null is to work around a bug where the overwrite doesnt work
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));

            string newFileName = "share_" + playlistId + ".playlist";

            var uploadResult = await liveClient.UploadAsync(folderId, newFileName, memoryStream, OverwriteOption.Overwrite);

            // get the id
            var fileId = uploadResult.Result["id"];

            // now get a share link
            var shareLink = await liveClient.GetAsync(fileId + "/shared_read_link");

            return (string)shareLink.Result["link"];
        }

        public string GetDownloadLink(string sharedLink)
        {
            if (sharedLink.Contains("redir.aspx"))
                return sharedLink.Replace("redir.aspx", "download.aspx");

            return string.Empty;
        }
    }

}
