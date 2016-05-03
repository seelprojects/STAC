using SourceCodeIndexer.UI.ViewModels;

namespace SourceCodeIndexer.UI
{
    /// <summary>
    /// Interaction logic for CustomDictionary.xaml
    /// </summary>
    public partial class CustomDictionaryWindow
    {
        public CustomDictionaryWindowViewModel ViewModel { get; set; }

        public CustomDictionaryWindow()
        {
            ViewModel = new CustomDictionaryWindowViewModel();
            DataContext = ViewModel;

            InitializeComponent();
        }

        /// <summary>
        /// Updates Add word button
        /// </summary>
        private void TxtAddWord_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            BtnAddWord.IsEnabled = !string.IsNullOrWhiteSpace(TxtAddWord.Text);
        }

        /// <summary>
        /// Enables delete selected button
        /// </summary>
        private void LstWords_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BtnDelete.IsEnabled = LstWords.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Adds word to list
        /// </summary>
        private void BtnAddWord_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.AddWord();
        }

        /// <summary>
        /// Closes this window
        /// </summary>
        private void BtnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Saves list
        /// </summary>
        private void BtnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        /// <summary>
        /// Deletes selected
        /// </summary>
        private void BtnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Delete(LstWords.SelectedItems);
        }

        /// <summary>
        /// Deletes all
        /// </summary>
        private void BtnDeleteAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.DeleteAll();
        }

        /// <summary>
        /// imports file
        /// </summary>
        private void BtnImport_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ImportFile();
        }
    }
}
