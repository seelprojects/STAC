using Xceed.Wpf.Toolkit;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SourceCodeIndexer.UI.ViewModels;

namespace SourceCodeIndexer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class IndexerWindow
    {
        public IndexerWindowViewModel ViewModel { get; set; }

        public IndexerWindow()
        {
            ViewModel = new IndexerWindowViewModel();
            DataContext = ViewModel;

            InitializeComponent();
        }

        #region Project TextBox

        private void TxtProjectPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TxtProjectPath.Text))
                ViewModel.LoadFiles(TxtProjectPath.Text);
        }

        private void TxtProjectPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowFolderDialog();
        }

        #endregion

        #region Project Buttons

        private void BtnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderDialog();
        }

        #endregion

        /// <summary>
        /// Loads Directory search
        /// </summary>
        private void ShowFolderDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            TxtProjectPath.Text = folderBrowserDialog.SelectedPath;
            ViewModel.LoadFiles(folderBrowserDialog.SelectedPath);
        }

        #region Move Files

        private void BtnMoveSelectedAvailableToSelected_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveSelectedAvailableToSelected(LstBoxAvailableFiles.SelectedItems);
            StartIndexingButtonEnabled();
        }

        private void BtnMoveAllAvailableToSelected_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveAllAvailableToSelected();
            StartIndexingButtonEnabled();
        }

        private void BtnMoveSelectedSelectedToAvailable_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveSelectedSelectedToAvailable(LstBoxSelectedFiles.SelectedItems);
            StartIndexingButtonEnabled();
        }

        private void BtnMoveAllSelectedToAvailable_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveAllSelectedToAvailable();
            StartIndexingButtonEnabled();
        }

        #endregion

        private void BtnStartIndexing_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.IndexFiles();
        }

        private void StartIndexingButtonEnabled()
        {
            ViewModel.UpdateCanStartIndexing();
        }

        private void ChkSplitOn_Click(object sender, RoutedEventArgs e)
        {
            StartIndexingButtonEnabled();
        }

        #region Radio buttons Stemmer/Lemmatizer

        private bool _radStemmerCheckedNow;
        private bool _radLemmatizerCheckedNow;

        private void RadioButtonStemmer_Click(object sender, RoutedEventArgs e)
        {
            if (_radStemmerCheckedNow)
            {
                _radStemmerCheckedNow = false;
                return;
            }

            RadStemmer.IsChecked = false;
        }

        private void RadioButtonStemmer_Checked(object sender, RoutedEventArgs e)
        {
            _radStemmerCheckedNow = true;
        }

        private void RadioButtonLemmatizer_Click(object sender, RoutedEventArgs e)
        {
            if (_radLemmatizerCheckedNow)
            {
                _radLemmatizerCheckedNow = false;
                return;
            }

            RadLemmatizer.IsChecked = false;
        }

        private void RadioButtonLemmatizer_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.TryInitializeLemmatizer())
            {
                _radLemmatizerCheckedNow = true;
            }
            else
            {
                _radLemmatizerCheckedNow = false;
                RadLemmatizer.IsChecked = false;
            }
        }

        #endregion

        #region Menu

        /// <summary>
        /// Opens custom dictionary
        /// </summary>
        private void CustomDictionaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DisplayCustomDictionary();
        }

        /// <summary>
        /// Opens Loc metric
        /// </summary>
        private void LocMetricMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DisplayLocMetric();
        }

        private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.HasIndexingResult)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;
                ViewModel.Export(folderBrowserDialog.SelectedPath);
            }
            else
            {
                System.Windows.MessageBox.Show("Select a project to index and click Start Indexing before saving a result.", "Result not found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DisplayAboutWindow();
        }

        #endregion
    }
}
