using System.Diagnostics;
using System.Windows.Navigation;

namespace SourceCodeIndexer.UI
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
