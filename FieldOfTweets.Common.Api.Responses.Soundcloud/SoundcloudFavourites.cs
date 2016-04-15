﻿using System;
using FieldOfTweets.Common.Api.Responses.Soundcloud;

namespace FieldOfTweets.Common.Api.Soundcloud.Responses
{
    
    public class ResponseSoundcloudFavourites
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int duration { get; set; }
        public bool? commentable { get; set; }
        public string state { get; set; }
        public int original_content_size { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public string description { get; set; }
        public bool? streamable { get; set; }
        public bool? downloadable { get; set; }
        public string genre { get; set; }
        public string release { get; set; }
        public string purchase_url { get; set; }
        public object purchase_title { get; set; }
        public int? label_id { get; set; }
        public string label_name { get; set; }
        public string isrc { get; set; }
        public string video_url { get; set; }
        public string track_type { get; set; }
        public string key_signature { get; set; }
        public double? bpm { get; set; }
        public string title { get; set; }
        public int? release_year { get; set; }
        public int? release_month { get; set; }
        public int? release_day { get; set; }
        public string original_format { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public string waveform_url { get; set; }
        public SoundcloudUser user { get; set; }
        public string stream_url { get; set; }
        public int user_playback_count { get; set; }
        public bool user_favorite { get; set; }
        public int playback_count { get; set; }
        public int download_count { get; set; }
        public int favoritings_count { get; set; }
        public int comment_count { get; set; }
        public string attachments_uri { get; set; }
        public string download_url { get; set; }
        public Label label { get; set; }
    }
}
