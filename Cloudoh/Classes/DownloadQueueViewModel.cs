using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cloudoh.Annotations;
using Cloudoh.ViewModels;
using Microsoft.Phone.BackgroundTransfer;

namespace Cloudoh.Classes
{
    public class DownloadQueueViewModel : INotifyPropertyChanged
    {
        private TransferStatus _status;
        private int _completion;

        public SoundcloudViewModel Tag { get; set; }

        public DownloadQueueViewModel(SoundcloudViewModel downloadQueueItem)
        {
            Tag = downloadQueueItem;
        }

        public TransferStatus Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public int Completion
        {
            get { return _completion; }
            set
            {
                if (value == _completion) return;
                _completion = value;
                OnPropertyChanged();
            }
        }

        public string RequestId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}