namespace FieldOfTweets.Common
{

    public class VersionInfo
    {

	    private static int Major
        {
            get { return 1; }
        }

        private static int Minor
        {
            get { return 12; }
        }

        private static long BuildNumber
        {
			get
			{
				return 2620;
			}
        }

		public static string FullVersion()
        {
			return string.Format("Version {0}.{1}.{2}", Major, Minor, BuildNumber);        
		}

    }

}
