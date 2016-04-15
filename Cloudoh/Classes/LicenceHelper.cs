using System;
using System.Threading.Tasks;
using System.Windows;
using Cloudoh.Common;
using Windows.ApplicationModel.Store;

namespace Cloudoh.Classes
{
    internal static class LicenceHelper
    {

        internal static bool IsLicensedForPremium()
        {                 
#if DEBUG            
         return true;
#else
            if (System.Diagnostics.Debugger.IsAttached)
                return true;

            var licence = CurrentApp.LicenseInformation.ProductLicenses[ApplicationConstants.CloudohPremiumLicence];
            return licence.IsActive;
#endif
        }

        public async static Task PromptPurchase()
        {
            var res = MessageBox.Show("In order to use some features, such as creating and sharing playlists, you need to upgrade to the premium edition of Cloudoh.\n\nWould you like to visit the store and upgrade?", "cloudoh premium", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.OK)
            {
                try
                {                    
                    var result = await CurrentApp.RequestProductPurchaseAsync(ApplicationConstants.CloudohPremiumLicence, false);
                }
                catch (Exception)
                {                                     
                }                
            }
        }

    }

}
