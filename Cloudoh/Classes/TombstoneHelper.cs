using Cloudoh.Common;
using Cloudoh.ViewModels;

namespace Cloudoh.Classes
{

    public class TombstoneHelper
    {

        public void SaveState()
        {
            var sh = new StorageHelper();

            sh.SaveContentsToFile("SoundcloudDashboard.state", App.ViewModel.SoundcloudDashboard);
            sh.SaveContentsToFile("SoundcloudFavourites.state", App.ViewModel.SoundcloudFavourites);
            sh.SaveContentsToFile("SoundcloudSearchResultsHot.state", App.ViewModel.SoundcloudSearchResultsHot);
            sh.SaveContentsToFile("SoundcloudSearchResultsNew.state", App.ViewModel.SoundcloudSearchResultsNew);
            
            sh.SaveContentsToFile("CurrentUserViewModel.state", App.ViewModel.CurrentUserViewModel);
            sh.SaveContentsToFile("CurrentSoundcloudProfile.state", App.ViewModel.CurrentSoundcloudProfile);
            sh.SaveContentsToFile("CurrentSoundcloud.state", App.ViewModel.CurrentSoundcloud);
            sh.SaveContentsToFile("CurrentPlaylist.state", App.ViewModel.CurrentPlaylist);

            sh.SaveContentsToFile("CurrentTrackToAdd.state", App.ViewModel.CurrentTrackToAdd);
        }

        public void LoadState()
        {
            var sh = new StorageHelper();

            App.ViewModel.CurrentUserViewModel = sh.LoadContentsFromFile<SoundcloudAccessViewModel>("CurrentUserViewModel.state");
            App.ViewModel.CurrentSoundcloudProfile = sh.LoadContentsFromFile<SoundcloudUserViewModel>("CurrentSoundcloudProfile.state");
            App.ViewModel.CurrentSoundcloud = sh.LoadContentsFromFile<SoundcloudViewModel>("CurrentSoundcloud.state");
            App.ViewModel.CurrentPlaylist = sh.LoadContentsFromFile<SoundcloudViewModel>("CurrentPlaylist.state");

            App.ViewModel.CurrentTrackToAdd = sh.LoadContentsFromFile<SoundcloudViewModel>("CurrentTrackToAdd.state");

            App.ViewModel.SoundcloudDashboard = sh.LoadContentsFromFile<SortedObservableCollection<SoundcloudViewModel>>("SoundcloudDashboard.state");
            App.ViewModel.SoundcloudFavourites = sh.LoadContentsFromFile<SortedObservableCollection<SoundcloudViewModel>>("SoundcloudFavourites.state");
            App.ViewModel.SoundcloudSearchResultsHot = sh.LoadContentsFromFile<SortedObservableCollection<SoundcloudViewModel>>("SoundcloudSearchResultsHot.state");
            App.ViewModel.SoundcloudSearchResultsNew = sh.LoadContentsFromFile<SortedObservableCollection<SoundcloudViewModel>>("SoundcloudSearchResultsNew.state");
        }

    }

}
