using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Resources;
using Cloudoh.Common.API.Soundcloud;
using Newtonsoft.Json;
using ProtoBuf;

namespace Cloudoh.Common
{

    public class StorageHelper
    {

        private const string UserFileName = "soundcloud.user";

        public static long UserId { get; set; }

        public void SaveUser(SoundcloudAccess soundcloudAccess)
        {

            // Obtain the virtual store for the application.
            using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myStore.FileExists(UserFileName))
                    myStore.DeleteFile(UserFileName);

                using (var isoFileStream = myStore.OpenFile(UserFileName, FileMode.CreateNew))
                {
                    var value = JsonConvert.SerializeObject(soundcloudAccess);
                    using (var sw = new StreamWriter(isoFileStream))
                    {
                        sw.Write(value);
                        sw.Flush();
                    }
                }

            }

        }


        private static object lockUser = new object();

        public SoundcloudAccess GetSoundcloudUser()
        {
            try
            {

                using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (!myStore.FileExists(UserFileName))
                        return null;

                    lock (lockUser)
                    {
                        using (var isoFileStream = myStore.OpenFile(UserFileName, FileMode.Open))
                        {
                            using (var sw = new StreamReader(isoFileStream))
                            {
                                var contents = sw.ReadToEnd();
                                return JsonConvert.DeserializeObject<SoundcloudAccess>(contents);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        private class RequestState
        {
            public string FullImageUrl { get; set; }
            public string ProfileImageUrl { get; set; }
        }

        public event EventHandler SaveProfileImageCompletedEvent;

        public void SaveProfileImage(string fullImageUrl, string actualFileName, long userId)
        {

            UserId = userId;

            var state = new RequestState
            {
                FullImageUrl = fullImageUrl,
                ProfileImageUrl = actualFileName
            };

            var client = new WebClient();
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.OpenReadAsync(new Uri(fullImageUrl), state);

        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {

            var requestState = e.UserState as RequestState;

            if (e.Error != null)
            {
                // The remote server returned an error: NotFound.
                if (!String.IsNullOrEmpty(e.Error.Message))
                {
                    if (e.Error.Message.ToLower().Contains("the remote server returned an error: notfound"))
                    {
                        if (requestState != null && (requestState.FullImageUrl != null && requestState.FullImageUrl.EndsWith("&size=original")))
                        {
                            var fullImageUrl = requestState.FullImageUrl.Replace("&size=original", "");
                            requestState.FullImageUrl = fullImageUrl;
                            var client = new WebClient();
                            client.OpenReadCompleted += client_OpenReadCompleted;
                            client.OpenReadAsync(new Uri(fullImageUrl), requestState);
                        }
                    }
                }

                return;
            }

            try
            {

                var resInfo = new StreamResourceInfo(e.Result, null);
                if (requestState != null)
                {
                    var profileImageUrl = requestState.ProfileImageUrl;

                    byte[] contents;

                    using (var reader = new StreamReader(resInfo.Stream))
                    {
                        using (var bReader = new BinaryReader(reader.BaseStream))
                        {
                            contents = bReader.ReadBytes((int)reader.BaseStream.Length);
                        }
                    }

                    SaveSharedImage(contents, profileImageUrl);

                }

                if (SaveProfileImageCompletedEvent != null)
                    SaveProfileImageCompletedEvent(null, null);

            }
            catch (Exception)
            {

            }

        }

        private static readonly object SaveSharedImageLock = new object();

        private static void SaveSharedImage(byte[] contents, string profileImageUrl)
        {

            lock (SaveSharedImageLock)
            {

                using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    var fileName = Path.GetExtension(profileImageUrl);
                    if (fileName != null && !fileName.StartsWith("."))
                        fileName = "." + fileName;

                    if (fileName == ".")
                        fileName = ".jpg";

                    // Delete any old images
                    var path1 = ApplicationConstants.UserStorageFolder + "/" + UserId + ".*";
                    ClearImagesFromPath(myStore, path1, ApplicationConstants.UserStorageFolder);

                    fileName = String.Format("{0}{1}", UserId, fileName);

                    string targetFile = ApplicationConstants.UserStorageFolder + "/" + fileName;

                    using (var isf = myStore.OpenFile(targetFile, FileMode.Create))
                    {
                        var writer = new BinaryWriter(isf);
                        writer.Write(contents);
                    }

                    var path2 = ApplicationConstants.ShellContentFolder + "/" + UserId + ".*";
                    ClearImagesFromPath(myStore, path2, ApplicationConstants.ShellContentFolder);

                    var otherTargetFile = ApplicationConstants.ShellContentFolder + "/" + fileName;

                    using (var writer = new BinaryWriter(new IsolatedStorageFileStream(otherTargetFile, FileMode.Create, myStore)))
                    {
                        writer.Write(contents);
                    }
                }

            }

        }

        private static void ClearImagesFromPath(IsolatedStorageFile myStore, string path, string rootPath)
        {

            try
            {

                if (!myStore.DirectoryExists(rootPath))
                {
                    myStore.CreateDirectory(rootPath);
                    return;
                }

                var res = myStore.GetFileNames(path);
                var existingFiles = res.Where(file => file.ToLower().EndsWith(".gif") || file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpeg") || file.ToLower().EndsWith("."));

                foreach (var existingFile in existingFiles)
                {
                    try
                    {
                        myStore.DeleteFile(rootPath + "/" + existingFile);
                    }
                    catch (Exception)
                    {
                    }
                }

            }
            catch (Exception)
            {
            }
        }


        private readonly object CachedImageUriLock = new object();

        public string CachedImageUri(string userId)
        {

            lock (CachedImageUriLock)
            {
                try
                {

                    // Obtain the virtual store for the application.
                    using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var path = ApplicationConstants.UserStorageFolder + "/" + userId + ".*";

                        var res = myStore.GetFileNames(path);
                        var fileName =
                            res.FirstOrDefault(
                                file =>
                                file.ToLower().EndsWith(".gif") || file.ToLower().EndsWith(".jpg") ||
                                file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpeg") ||
                                file.ToLower().EndsWith("."));

                        if (string.IsNullOrEmpty(fileName))
                            return null;

                        return "/" + ApplicationConstants.UserStorageFolder + "/" + fileName;
                    }

                }
                catch (Exception)
                {
                    return null;
                }

            }

        }

        public void SaveContentsToFile<T>(string filePath, T obj) where T : new()
        {

            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var newFile = storage.OpenFile(filePath, FileMode.Create))
                    {
                        Serializer.Serialize(newFile, obj);
                    }
                }

            }
            catch (Exception)
            {
                // ignore?!?
            }

        }

        public T LoadContentsFromFile<T>(string filePath) where T : new()
        {

            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!storage.FileExists(filePath))
                        return default(T);

                    using (var existingFile = storage.OpenFile(filePath, FileMode.Open))
                    {
                        var obj = Serializer.Deserialize<T>(existingFile);
                        return obj;
                    }
                }

            }
            catch (Exception)
            {
                // ignore?!?
            }

            return default(T);
        }

        public string LoadContentsFromFile(string filePath)
        {
            string contents = string.Empty;

            try
            {

                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (storage.FileExists(filePath))
                    {
                        using (var newFile = storage.OpenFile(filePath, FileMode.Open))
                        {
                            using (var reader = new StreamReader(newFile))
                            {
                                contents = reader.ReadToEnd();
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                contents = string.Empty;
            }


            return contents;
        }


        public string SerialiseResponseObject<T>(T settings)
        {

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, settings);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }

        }


        public void WriteShellTileImage()
        {
            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var fileName = Path.Combine(ApplicationConstants.UserCacheStorageFolder, "tile_music.png");
                    using (var file = storage.OpenFile(fileName, FileMode.Create))
                    {
                        StreamResourceInfo sri = Application.GetResourceStream(new Uri("/Cloudoh;component/tile_music_resource.png", UriKind.Relative));
                        var stream = sri.Stream;
                        CopyStream(stream, file);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void CopyStream(Stream input, Stream output)
        {
            // Insert null checking here for production
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        public Uri[] CopyAlbumArtToShellFolder(Uri[] images)
        {

            var uris = new List<Uri>();


            // Clear the existing folder
            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var files = storage.GetFileNames("/Shared/ShellContent/tile_*");
                    foreach (var file in files)
                        storage.DeleteFile("/Shared/ShellContent/" + file);
                }

            }
            catch (Exception)
            {
            }

            var sh = new SettingsHelper();
            var settings = sh.GetSettings();

            if (settings.ShowAlbumArtOnTile)
            {
                foreach (var image in images)
                {

                    try
                    {
                        // copies files from one location to /shared/shellcontent
                        using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            var fileName = Path.GetFileName(image.ToString());
                            var targetFile = "/Shared/ShellContent/tile_" + fileName;
                            storage.CopyFile(image.ToString(), targetFile, true);
                            uris.Add(new Uri("isostore:" + targetFile, UriKind.Absolute));
                        }

                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                }
            }
            else
            {
                try
                {

                    WriteShellTileImage();

                    // copies files from one location to /shared/shellcontent
                    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        const string fileName = "tile_music.png";
                        const string targetFile = "/Shared/ShellContent/tile_" + fileName;
                        storage.CopyFile(Path.Combine(ApplicationConstants.UserCacheStorageFolder, fileName), targetFile, true);
                        uris.Add(new Uri("isostore:" + targetFile, UriKind.Absolute));
                    }

                }
                catch (Exception)
                {
                    // ignore
                }

            }

            return uris.ToArray();

        }

        private static readonly object PlaylistLock = new object();

        public void SaveCustomPlaylists(object playlists)
        {
            lock (PlaylistLock)
            {
                SaveContentsToFile(ApplicationConstants.SoundcloudPlaylists, playlists);
            }
        }

    }

}
