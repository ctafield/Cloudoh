using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ViewModels;
using CrittercismSDK;
using Microsoft.Phone.BackgroundTransfer;
using Newtonsoft.Json;

namespace Cloudoh.Classes
{

    internal class DownloadHelper
    {


        public IList<DownloadQueueViewModel> GetAllDownloads()
        {
            var requests = new List<DownloadQueueViewModel>();

            foreach (var request in BackgroundTransferService.Requests)
            {
                var tag = request.Tag;
                var downloadQueueItem = JsonConvert.DeserializeObject<SoundcloudViewModel>(tag);

                var item = new DownloadQueueViewModel(downloadQueueItem)
                {
                    Status = request.TransferStatus,
                    RequestId = request.RequestId,
                    Completion = (int)((100 / (double)request.TotalBytesToReceive) * request.BytesReceived)
                };

                requests.Add(item);
            }

            return requests;
        }

        public IList<DownloadQueueViewModel> GetFinishedDownloads()
        {
            return GetAllDownloads().Where(x => x.Status == TransferStatus.Completed).ToList();
        }

        public IList<DownloadQueueViewModel> GetActiveDownloadQueue()
        {
            return GetAllDownloads().Where(x => x.Status != TransferStatus.Completed).ToList();
        }

        /// <summary>
        /// Returns true or false dependant upon if the file was added.
        /// </summary>
        /// <returns></returns>
        public string AddDownloadLink(SoundcloudViewModel downloadTrack)
        {
            EnsureDirectoryExists();

            // Max 25 files
            if (BackgroundTransferService.Requests.Count() >= 25)
            {
                return "Maximum of 25 items allowed in the download queue.";
            }

            // Can't download it
            if (!downloadTrack.Downloadable || string.IsNullOrWhiteSpace(downloadTrack.DownloadUrl))
            {
                return "This track cannot be downloaded from SoundCloud.";
            }

            // already in the queue ?
            if (BackgroundTransferService.Requests.Any(x => x.RequestUri.OriginalString == downloadTrack.DownloadUrl))
            {
                return "This track has already been downloaded, or is being downloaded.";
            }

            var downloadUri = new Uri(GetFormattedDownloadUrl(downloadTrack.DownloadUrl), UriKind.Absolute);
            var downloadLocation = new Uri(ApplicationConstants.DownloadFolder + "/" + downloadTrack.Id, UriKind.Relative);
            var newRequest = new BackgroundTransferRequest(downloadUri, downloadLocation)
                             {
                                 Tag = downloadTrack.Clone().AsJson(),
                                 Method = "GET"
                             };

            try
            {
                BackgroundTransferService.Add(newRequest);
            }
            catch (Exception exception)
            {
                Crittercism.LogHandledException(exception);
                return "This track has already been downloaded, or is being downloaded.";
            }
            
            return null;

        }

        private string GetFormattedDownloadUrl(string downloadTrack)
        {
            var sc = new SoundcloudApi();

            if (downloadTrack.Contains("?"))
            {
                var url = downloadTrack.Replace("https", "http")  + "&client_id=" + sc.ClientId;
                return url;
            }
            else
            {
                var url = downloadTrack.Replace("https", "http") + "?client_id=" + sc.ClientId;
                return url;                
            }
        }

        /// <summary>
        /// Make sure that the required "/shared/transfers" directory exists in isolated storage.
        /// </summary>
        private void EnsureDirectoryExists()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists(ApplicationConstants.DownloadFolder))
                {
                    isoStore.CreateDirectory(ApplicationConstants.DownloadFolder);
                }
            }
        }

        public void DeleteDownload(string requestId, long trackId)
        {
            var download = BackgroundTransferService.Find(requestId);
            if (download != null)
            {
                BackgroundTransferService.Remove(download);

                // now delete the file
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(ApplicationConstants.DownloadFolder + "/" + trackId))
                        isoStore.DeleteFile(ApplicationConstants.DownloadFolder + "/" + trackId);
                }
            }
        }

        public void DeleteAllDownloads()
        {
            var finishedDownloads = GetFinishedDownloads();
            foreach (var finishedDownload in finishedDownloads)
            {
                DeleteDownload(finishedDownload.RequestId, finishedDownload.Tag.Id);
            }
        }

        public Uri GetRemoteOrCachedUrl(long trackId, string streamingUrl)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(ApplicationConstants.DownloadFolder + "/" + trackId))
                {
                    return new Uri(ApplicationConstants.DownloadFolderNoSlash + "/" + trackId, UriKind.Relative);
                }
            }
            return new Uri(streamingUrl, UriKind.Absolute);
        }

    }
}
