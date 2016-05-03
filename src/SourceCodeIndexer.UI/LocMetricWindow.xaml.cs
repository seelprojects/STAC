using SourceCodeIndexer.UI.ViewModels;

namespace SourceCodeIndexer.UI
{
    /// <summary>
    /// Interaction logic for LocMetric.xaml
    /// </summary>
    public partial class LocMetricWindow
    {
        public LocMetricWindowViewModel ViewModel { get; set; }

        public LocMetricWindow()
        {
            ViewModel = new LocMetricWindowViewModel();
            DataContext = ViewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Closes
        /// </summary>
        private void BtnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
