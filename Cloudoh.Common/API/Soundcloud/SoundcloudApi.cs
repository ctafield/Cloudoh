// *********************************************************************************************************
// <copyright file="SearchPage.xaml.cs" company="My Own Limited">
// Copyright (c) 2013 All Rights Reserved
// </copyright>
// <summary> Mehdoh for Windows Phone </summary>
// *********************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Cloudoh.Common.ErrorLogging;
using FieldOfTweets.Common.Api;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Retries;
using Hammock.Silverlight.Compat;
using Hammock.Web;

namespace Cloudoh.Common.API.Soundcloud
{
    public class SoundcloudApi : MehdohApi
    {

        #region Constructor

        private SoundcloudAccess UserAccess { get; set; }
        public SoundcloudOAuthResponse OAuthResponse { get; set; }
        public SoundcloudUserProfile UserProfile { get; set; }

        public SoundcloudApi()
        {
            var storageHelper = new StorageHelper();
            var user = storageHelper.GetSoundcloudUser();

            if (user == null)
                return;

            UserAccess = user;
        }

        #endregion

        #region Properties


        private string BaseUrl
        {
            get { return "https://api.soundcloud.com"; }
        }

        public string CallbackUri
        {
            get { return "http://www.myownltd.co.uk/cloudoh/oauth/callback.html"; }
        }

        public string ClientId
        {
            // Cloudoh
            get { return "4bb367c7a85f3c17eb3e5b2381c8d5ce"; }
        }

        public string ClientSecret
        {
            // Cloudoh 
            get { return "514e28d55d27c239f7e2545583e33978"; }
        }

        public string LoginUrl
        {
            get
            {
                return string.Format("https://soundcloud.com/connect?client_id={0}&redirect_uri={1}&response_type=code_and_token&scope=non-expiring&display=popup",
                        ClientId, HttpUtility.UrlEncode(CallbackUri));
            }
        }

        #endregion

        private async Task<bool> CallClientStatus(RestClient client, RestRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();

            client.BeginRequest(request, new RestCallback(
                delegate(RestRequest req, RestResponse resp, object userState)
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                        tcs.SetResult(true);
                    else
                        tcs.SetResult(false);
                }));

            // wait here until the stuff above has finished.
            await tcs.Task;

            return tcs.Task.Result;
        }

        private async Task CallClient(RestClient client, RestRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();

            client.BeginRequest(request, new RestCallback(
                delegate(RestRequest req, RestResponse resp, object userState)
                {
                    tcs.SetResult(true);
                }));

            // wait here until the stuff above has finished.
            await tcs.Task;
        }

        private async Task<T> CallClient<T>(RestClient client, RestRequest request)
        {

            var tcs = new TaskCompletionSource<T>();

            client.BeginRequest(request, new RestCallback(delegate(RestRequest req, RestResponse resp, object userState)
            {
                try
                {
                    T result = DeserialiseResponse<T>(resp);
                    tcs.SetResult(result);
                }
                catch (Exception)
                {
                    // Dont care
                    tcs.SetResult(default(T));
                }
            }));

            // wait here until the stuff above has finished.
            await tcs.Task;

            return tcs.Task.Result;

        }


        #region Members

        private bool IsValidResponse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            return true;
        }
        
        public string GetTrackUrl(string input)
        {
            // TODO: Fix this when MS fix their crap
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            return string.Format("{0}?client_id={1}", input.Replace("https://", "http://"), ClientId);
        }

        public event EventHandler GetAuthTokenCompletedEvent;

        public void GetAuthToken(string code)
        {
            // client_id: your client id
            // client_secret: your client secret
            // grant_type: authorization_code is currently the only supported value
            // redirect_uri: the redirect_uri you used in the authorization request. Note: this has to be exactly the same value as in the authorization request.
            // code: the exact CODE you received during the authorization step.

            var request = new RestRequest
                              {
                                  Method = WebMethod.Post
                              };

            request.AddParameter("client_id", ClientId);
            request.AddParameter("client_secret", ClientSecret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", CallbackUri);
            request.AddParameter("code", code);

            request.Path = BaseUrl + "/oauth2/token";

            var client = GetClient();
            client.BeginRequest(request, GetAuthTokenCompleted);
        }


        private void GetAuthTokenCompleted(RestRequest request, RestResponse response, object userstate)
        {

            try
            {
                OAuthResponse = DeserialiseResponse<SoundcloudOAuthResponse>(response);

                if (OAuthResponse == null || string.IsNullOrEmpty(OAuthResponse.access_token))
                {
                    // bail out
                    SafeRaiseEvent(GetAuthTokenCompletedEvent);
                }

                // now get the user
                GetUserCompletedEvent += GetUserCompletedForOauth;

                GetUser(OAuthResponse.access_token);

            }
            catch (Exception)
            {
                // bail 
                SafeRaiseEvent(GetAuthTokenCompletedEvent);
            }

        }

        private void GetUserCompletedForOauth(object sender, EventArgs e)
        {
            SafeRaiseEvent(GetAuthTokenCompletedEvent);
        }

        public void GetUser(string accessToken)
        {
            // /me.json?oauth_token=A_VALID_TOKEN"

            var request = new RestRequest
                              {
                                  Method = WebMethod.Get
                              };

            if (!string.IsNullOrEmpty(accessToken))
                request.AddParameter("oauth_token", accessToken);
            else
                request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + "/me.json";

            var client = GetClient();
            client.BeginRequest(request, GetUserCompleted);
        }

        public event EventHandler GetUserCompletedEvent;

        private void GetUserCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                UserProfile = DeserialiseResponse<SoundcloudUserProfile>(response);
            }
            finally
            {
                SafeRaiseEvent(GetUserCompletedEvent);
            }
        }

        private void SafeRaiseEvent(EventHandler eventHandler)
        {
            if (eventHandler != null)
                eventHandler(this, null);
        }

        #endregion

        #region Dashboard

        #region Properties

        public ResponseDashboard Dashboard { get; set; }

        #endregion

        #region Members

        private RestClient GetClient()
        {
            var client = new RestClient
            {
                RetryPolicy = new RetryPolicy() { RetryCount = 3 },
                DecompressionMethods = DecompressionMethods.GZip,
                UserAgent = "cloudoh"
            };

            return client;
        }

        public bool FetchingMoreDashboard { get; private set; }

        public async Task<ResponseDashboard> GetDashboard(long? offset = 0)
        {
            // /me.json?oauth_token=A_VALID_TOKEN"

            var request = new RestRequest
                              {
                                  Method = WebMethod.Get
                              };

            request.AddParameter("limit", "100");
            request.AddParameter("oauth_token", UserAccess.AccessToken);

            if (offset.HasValue && offset > 0)
            {
                request.AddParameter("offset", offset.Value.ToString(CultureInfo.InvariantCulture));
                FetchingMoreDashboard = true;
            }
            else
            {
                FetchingMoreDashboard = false;
            }

            request.Path = BaseUrl + "/me/activities/all.json";

            var client = GetClient();
            return await CallClient<ResponseDashboard>(client, request);
        }

        #endregion

        #endregion

        #region Favourites

        #region Members

        public bool FetchingMoreFavourites { get; private set; }

        public async Task<List<ResponseGetTrack>>  GetFavourites(long? offset = 0)
        {
            // /me.json?oauth_token=A_VALID_TOKEN"

            var request = new RestRequest
                              {
                                  Method = WebMethod.Get
                              };

            request.AddParameter("oauth_token", UserAccess.AccessToken);

            if (offset.HasValue && offset > 0)
            {
                request.AddParameter("offset", offset.Value.ToString(CultureInfo.InvariantCulture));
                FetchingMoreFavourites = true;
            }
            else
            {
                FetchingMoreFavourites = false;
            }

            request.Path = BaseUrl + "/me/favorites.json";

            var client = GetClient();
            //client.BeginRequest(request, GetFavouritesCompleted);
            return await CallClient<List<ResponseGetTrack>>(client, request);

        }

        #endregion

        #endregion

        #region Get Track

        #region Properties

        public ResponseGetTrack TrackDetails { get; set; }

        #endregion

        #region Members

        public void GetTrack(string id)
        {
            var request = new RestRequest
                              {
                                  Method = WebMethod.Get
                              };

            request.AddParameter("client_id", ClientId);

            request.Path = BaseUrl + "/tracks/" + id + ".json";

            var client = GetClient();
            client.BeginRequest(request, GetTrackCompleted);
        }

        public event EventHandler GetTrackCompletedEvent;

        private void GetTrackCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                TrackDetails = DeserialiseResponse<ResponseGetTrack>(response);
            }
            finally
            {
                SafeRaiseEvent(GetTrackCompletedEvent);
            }
        }

        #endregion

        #endregion

        #region Get Is Favourite

        #region Members

        public async Task<bool> GetIsFavourite(long id)
        {
            var request = new RestRequest
                              {
                                  Method = WebMethod.Get
                              };

            //request.AddParameter("client_id", ClientId);
            request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + string.Format("/me/favorites/{0}", id);

            var client = GetClient();
            return await CallClientStatus(client, request);            
        }

        #endregion

        #endregion

        #region Remove From Favourites

        public void RemoveFromFavourites(long id)
        {
            var request = new RestRequest
                              {
                                  Method = WebMethod.Delete
                              };

            //request.AddParameter("client_id", ClientId);
            request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + string.Format("/me/favorites/{0}", id);

            var client = GetClient();
            client.BeginRequest(request, RemoveFromFavouritesCompleted);
        }

        public event EventHandler RemoveFromFavouritesCompletedEvent;

        private void RemoveFromFavouritesCompleted(RestRequest request, RestResponse response, object userstate)
        {
            SafeRaiseEvent(RemoveFromFavouritesCompletedEvent);
        }

        #endregion

        #region Add To Favourites

        public void AddToFavourites(long id)
        {
            var request = new RestRequest
                              {
                                  Method = WebMethod.Put
                              };

            //request.AddParameter("client_id", ClientId);
            request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + string.Format("/me/favorites/{0}", id);

            var client = GetClient();
            client.BeginRequest(request, AddToFavouritesCompleted);
        }

        public event EventHandler AddToFavouritesCompletedEvent;

        private void AddToFavouritesCompleted(RestRequest request, RestResponse response, object userstate)
        {
            SafeRaiseEvent(AddToFavouritesCompletedEvent);
        }

        #endregion

        #region Get User Profile


        public async Task<SoundcloudUserProfile> GetUserProfile(long userId)
        {
            // http://api.soundcloud.com/users/3207.json?client_id=YOUR_CLIENT_ID

            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + userId.ToString(CultureInfo.InvariantCulture) + ".json";

            var client = GetClient();            
            return await CallClient<SoundcloudUserProfile>(client, request);
        }


        #endregion

        #region User Favourites

        public async Task<List<ResponseGetTrack>> GetUserFavorites(long userId)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + userId.ToString(CultureInfo.InvariantCulture) + "/favorites.json";

            var client = GetClient();                     
            return await CallClient<List<ResponseGetTrack>>(client, request);
        }

        #endregion

        public void GetUserTracks(long userId)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + userId.ToString(CultureInfo.InvariantCulture) + "/tracks.json";

            var client = GetClient();
            client.BeginRequest(request, GetUserTracksCompleted);
            
        }

        public event EventHandler GetUserTracksCompletedEvent;

        public List<ResponseGetTrack> Tracks { get; set; }

        private void GetUserTracksCompleted(RestRequest request, RestResponse response, object userstate)
        {

            try
            {
                if (!IsValidResponse(response.Content))
                {
                    return;
                }

                Tracks = DeserialiseResponse<List<ResponseGetTrack>>(response.Content);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("GetUserTracksCompleted", ex);
            }
            finally
            {
                SafeRaiseEvent(GetUserTracksCompletedEvent);
            }
                
        }

#region SearchByTrack

        public void SearchByTrack(string trackName, string order, bool? filterDownloadable, TimeSpan? minLength, TimeSpan? maxLength, short? minBpm, short? maxBpm)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.AddParameter("q", string.IsNullOrEmpty(trackName) ? "*" : trackName);

            string filters;

            if (filterDownloadable.GetValueOrDefault())
                filters= "downloadable";
            else
                filters = "streamable";
            
            request.AddParameter("filter", filters);                
            request.AddParameter("order", order);

            if (minLength.HasValue && minLength.GetValueOrDefault().TotalMilliseconds > 0)
                request.AddParameter("duration[from]", minLength.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            if (maxLength.HasValue && maxLength.GetValueOrDefault().TotalMilliseconds > 0)
                request.AddParameter("duration[to]", maxLength.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            if (minBpm.HasValue && minBpm.Value > 0)
                request.AddParameter("bpm[from]", minBpm.Value.ToString(CultureInfo.InvariantCulture));

            if (maxBpm.HasValue && maxBpm.Value > 0)
                request.AddParameter("bpm[to]", maxBpm.Value.ToString(CultureInfo.InvariantCulture));

            request.Path = BaseUrl + "/tracks.json";

            var client = GetClient();
            client.BeginRequest(request, SearchByTrackCompleted);           
        }

        public event EventHandler SearchByTrackCompletedEvent;
        public List<ResponseGetTrack> SearchTracks { get; set; }

        private void SearchByTrackCompleted(RestRequest request, RestResponse response, object userstate)
        {

            try
            {
                if (!IsValidResponse(response.Content))
                {
                    return;
                }

                SearchTracks = DeserialiseResponse<List<ResponseGetTrack>>(response.Content);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("GetUserTracksCompleted", ex);
            }
            finally
            {
                SafeRaiseEvent(SearchByTrackCompletedEvent);
            }

        }

#endregion

        public void SearchByUser(string query)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.AddParameter("q", query);

            request.Path = BaseUrl + "/users.json";

            var client = GetClient();
            client.BeginRequest(request, SearchByUserCompleted);                      
        }

        public List<SoundcloudUserProfile> SearchUsers { get; private set; }
        public event EventHandler SearchByUserCompletedEvent;

        private void SearchByUserCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                if (!IsValidResponse(response.Content))
                {
                    return;
                }

                SearchUsers = DeserialiseResponse<List<SoundcloudUserProfile>>(response.Content);

            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("SearchByUserCompleted", ex);
            }
            finally
            {
                SafeRaiseEvent(SearchByUserCompletedEvent);
            }               
        }

        public async Task<List<SoundcloudComment>> GetComments(long trackId)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/tracks/" + trackId.ToString(CultureInfo.InvariantCulture) + "/comments.json";

            var client = GetClient();
            return await CallClient<List<SoundcloudComment>>(client, request);

        }

        public void SearchByGenre(string genre, string order, bool? filterDownloadable, TimeSpan? minLength, TimeSpan? maxLength, short? minBpm, short? maxBpm)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            string filters = "streamable";
            if (filterDownloadable.GetValueOrDefault())
                filters = "downloadable";

            request.AddParameter("filter", filters);

            request.AddParameter("genres", genre.ToLowerInvariant());
            request.AddParameter("order", order);

            if (minLength.HasValue && minLength.GetValueOrDefault().TotalMilliseconds > 0)
                request.AddParameter("duration[from]", minLength.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            if (maxLength.HasValue && maxLength.GetValueOrDefault().TotalMilliseconds > 0)
                request.AddParameter("duration[to]", maxLength.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            
            if (minBpm.HasValue && minBpm.Value > 0)
                request.AddParameter("bpm[from]", minBpm.Value.ToString(CultureInfo.InvariantCulture));

            if (maxBpm.HasValue && maxBpm.Value > 0)
                request.AddParameter("bpm[to]", maxBpm.Value.ToString(CultureInfo.InvariantCulture));

            request.Path = BaseUrl + "/tracks.json";

            var client = GetClient();
            client.BeginRequest(request, SearchByTrackCompleted);
        }

        public async Task<ResponsePlaylist> GetTracksForPlaylist(long id)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/playlists/" +id.ToString(CultureInfo.InvariantCulture) + ".json";

            var client = GetClient();
            return await CallClient<ResponsePlaylist>(client, request);            
        }

        #region Does User Follow User

        public void DoesUserFollowUser(string thisUser, long theOtherUserId)
        {

            // /users/{id}/followings/{id}

            var credentials = new OAuthCredentials()
                                  {
                                      ConsumerKey = ClientId,
                                      ConsumerSecret = ClientSecret,
                                      Token = UserAccess.AccessToken
                                  };

            var request = new RestRequest
            {
                Method = WebMethod.Get                ,
                Credentials = credentials
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + thisUser + "/followings/" + theOtherUserId.ToString(CultureInfo.InvariantCulture) + ".json";

            var client = GetClient();
            client.BeginRequest(request, DoesUserFollowUserCompleted);

        }

        public bool DoesUserFollow { get; set; }
        public event EventHandler DoesUserFollowUserCompletedEvent;

        private void DoesUserFollowUserCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                DoesUserFollow = response.StatusCode == HttpStatusCode.OK;
            }
            finally
            {
                SafeRaiseEvent(DoesUserFollowUserCompletedEvent);
            }

        }

        #endregion

        #region Follow User

        public void FollowUser(string thisUser, long theOtherUserId)
        {
            // /users/{id}/followings/{id}

            var credentials = new OAuthCredentials()
            {
                ConsumerKey = ClientId,
                ConsumerSecret = ClientSecret,
                Token = UserAccess.AccessToken
            };

            var request = new RestRequest
            {
                Method = WebMethod.Put
                //Credentials = credentials
            };

            request.AddParameter("oauth_token", UserAccess.AccessToken);

            //request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + thisUser + "/followings/" + theOtherUserId.ToString(CultureInfo.InvariantCulture); // +".json";

            var client = GetClient();
            client.BeginRequest(request, FollowUserCompleted);

        }

        public event EventHandler FollowUserCompletedEvent;

        private void FollowUserCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {

            }
            catch (Exception)
            {
            }
            finally
            {
                SafeRaiseEvent(FollowUserCompletedEvent);
            }

        }

        #endregion

        #region UnFollow User

        public void UnFollowUser(string thisUser, long theOtherUserId)
        {
            // /users/{id}/followings/{id}

            var credentials = new OAuthCredentials()
            {
                ConsumerKey = ClientId,
                ConsumerSecret = ClientSecret,
                Token = UserAccess.AccessToken
            };

            var request = new RestRequest
            {
                Method = WebMethod.Delete
                //Credentials = credentials
            };

            //request.AddParameter("consumer_key", ClientId);

            request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + "/users/" + thisUser + "/followings/" + theOtherUserId.ToString(CultureInfo.InvariantCulture); // +".json";

            var client = GetClient();
            client.BeginRequest(request, UnFollowUserCompleted);

        }

        public event EventHandler UnFollowUserCompletedEvent;

        private void UnFollowUserCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeRaiseEvent(UnFollowUserCompletedEvent);
            }

        }

        #endregion

        #region Get All Following

        public void GetAllFollowing(long thisUser)
        {

            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + thisUser + "/followings.json";

            var client = GetClient();
            client.BeginRequest(request, GetAllFollowingCompleted);
           
        }

        public event EventHandler GetAllFollowingCompletedEvent;

        public List<SoundcloudUserProfile> Following { get; set; }

        private void GetAllFollowingCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                Following = DeserialiseResponse<List<SoundcloudUserProfile>>(response);
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeRaiseEvent(GetAllFollowingCompletedEvent);
            }
        }

        #endregion

        #region Get All Followers

        public void GetAllFollowers(long thisUser)
        {

            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + thisUser + "/followers.json";

            var client = GetClient();
            client.BeginRequest(request, GetAllFollowersCompleted);

        }

        public event EventHandler GetAllFollowersCompletedEvent;

        public List<SoundcloudUserProfile> Followers { get; set; }

        private void GetAllFollowersCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {
                Followers = DeserialiseResponse<List<SoundcloudUserProfile>>(response);
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeRaiseEvent(GetAllFollowersCompletedEvent);
            }

        }

        
        #endregion

        #region Post Comment

        public event EventHandler PostCommentCompletedEvent;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackId">Track ID</param>
        /// <param name="comment">Comment</param>
        /// <param name="timestamp">Timestamp is in milliseconds</param>
        public void PostComment(int trackId, string comment, int timestamp)
        {

            var request = new RestRequest
            {
                Method = WebMethod.Post
            };

            request.AddParameter("comment[body]", comment);
            request.AddParameter("comment[timestamp]", timestamp.ToString(CultureInfo.InvariantCulture));

            request.AddParameter("oauth_token", UserAccess.AccessToken);

            request.Path = BaseUrl + "/tracks/" + trackId + "/comments.json";

            var client = GetClient();
            client.BeginRequest(request, PostCommentCompleted);
            
        }

        private void PostCommentCompleted(RestRequest request, RestResponse response, object userstate)
        {
            try
            {

            }
            catch (Exception)
            {
            }
            finally
            {
                SafeRaiseEvent(PostCommentCompletedEvent);
            }
        }

        #endregion

        #region Users Playlists

        //public event EventHandler GetUsersPlaylistsCompletedEvent;

        public async Task<List<ResponsePlaylist>>  GetUsersPlaylists(string userId)
        {
            var request = new RestRequest
            {
                Method = WebMethod.Get
            };

            if (string.IsNullOrEmpty(userId))
                userId = UserAccess.Id;

            request.AddParameter("consumer_key", ClientId);

            request.Path = BaseUrl + "/users/" + userId.ToString(CultureInfo.InvariantCulture) + "/playlists.json";

            var client = GetClient();
            return await CallClient<List<ResponsePlaylist>>(client, request);
        }

        //public List<ResponsePlaylist> UsersPlaylists { get; set; }

        //private void GetUsersPlaylistsCompleted(RestRequest request, RestResponse response, object userstate)
        //{
        //    try
        //    {
        //        if (!IsValidResponse(response.Content))
        //        {
        //            return;
        //        }

        //        UsersPlaylists = DeserialiseResponse<List<ResponsePlaylist>>(response.Content);

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLogger.LogException("GetUsersPlaylistsCompleted", ex);
        //    }
        //    finally
        //    {
        //        SafeRaiseEvent(GetUsersPlaylistsCompletedEvent);
        //    }               

        //}
        
        #endregion

    }

}