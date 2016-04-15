using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cloudoh.ViewModels
{

    public class FollowsViewModel
    {

        public ObservableCollection<SoundcloudUserViewModel> Following { get; set; }
        public ObservableCollection<SoundcloudUserViewModel> Followers { get; set; }

        public FollowsViewModel()
        {
            Followers = new ObservableCollection<SoundcloudUserViewModel>();
            Following = new ObservableCollection<SoundcloudUserViewModel>();
        }

    }

}