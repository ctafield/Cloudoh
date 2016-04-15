using System.Net;

namespace Cloudoh.Common
{
    public class NokiaMusicConstants
    {

        public string ClientId
        {
            get
            {
                return "05137a4473a335467e24969bea974ffa";
            }
        }
        
        public string ClientSecret
        {
            get
            {
                return "TLupzQS2qFp240Al8IxHu1OEAqiRJiPPfG+xnimKeZsL6DTiTTYbc48biwCk6EwM";
            }
        }

        public string ApiBaseUrl
        {
            get 
            { 
                return "http://api.ent.nokia.com/1.x/us/?";
            }
        }

        public string GetArtistImage(string artistName)
        {
            return string.Format("http://api.ent.nokia.com/1.x/us/creators/images/320x320/random?domain=music&name={0}&client_id={1}",
                                 HttpUtility.UrlEncode(artistName),
                                 ClientId);
        }

    }
}
