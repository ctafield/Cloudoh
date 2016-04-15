using System;
using System.IO;
using System.Net;
using Microsoft.Phone.BackgroundAudio;

namespace Cloudoh.Audio.Agent
{

    /// <summary>
    /// A background agent that performs per-track streaming for playback
    /// </summary>
    public class SoundcloudAudioTrackStreamer : AudioStreamingAgent
    {
        /// <summary>
        /// Called when a new track requires audio decoding
        /// (typically because it is about to start playing)
        /// </summary>
        /// <param name="track">
        /// The track that needs audio streaming
        /// </param>
        /// <param name="streamer">
        /// The AudioStreamer object to which a MediaStreamSource should be
        /// attached to commence playback
        /// </param>
        /// <remarks>
        /// To invoke this method for a track set the Source parameter of the AudioTrack to null
        /// before setting  into the Track property of the BackgroundAudioPlayer instance
        /// property set to true;
        /// otherwise it is assumed that the system will perform all streaming
        /// and decoding
        /// </remarks>
        protected override void OnBeginStreaming(AudioTrack track, AudioStreamer streamer)
        {

            var request = WebRequest.CreateHttp(track.Tag);
            request.AllowReadStreamBuffering = true;

            IAsyncResult result = request.BeginGetResponse(delegate(IAsyncResult asyncResult)
            {
                HttpWebResponse response = request.EndGetResponse(asyncResult) as HttpWebResponse;
                Stream s = response.GetResponseStream();

                //var mss = new Mp3MediaStreamSource(s, response.ContentLength);

                // Event handler for when a track is complete or the user switches tracks
                //mss.StreamComplete += new EventHandler(mss_StreamComplete);
                // Set the source
                //streamer.SetSource(mss);

                //TODO: Set the SetSource property of streamer to a MSS source
                //NotifyComplete();
            }, null);


        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// The call to base.OnCancel() is necessary to release the background streaming resources
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();
        }

    }

}
