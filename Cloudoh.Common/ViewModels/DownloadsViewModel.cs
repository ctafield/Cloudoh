using System.Collections.ObjectModel;
using Cloudoh.Classes;

namespace Cloudoh.ViewModels
{
    
    public class DownloadsViewModel
    {
        public ObservableCollection<DownloadQueueViewModel> ActiveDownloadQueue { get; set; }
        public ObservableCollection<DownloadQueueViewModel> FinishedDownloadQueue { get; set; }
    }

}