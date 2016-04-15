using System.Windows.Controls;
using System.Windows.Media;

namespace Cloudoh.UserControls
{
    public partial class PanScanImage : UserControl
    {

        public ImageSource Source
        {
            set { panImage.Source = value; }
        }

        public PanScanImage()
        {
            InitializeComponent();
        }
    }
}
