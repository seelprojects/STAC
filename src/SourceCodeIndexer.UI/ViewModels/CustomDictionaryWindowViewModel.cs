using System;
using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using SourceCodeIndexer.STAC.Dictionaries;

namespace SourceCodeIndexer.UI.ViewModels
{
    public class CustomDictionaryWindowViewModel : ViewModelBase
    {
        private readonly Dictionary _dictionary;

        #region Properties

        public string Word { get; set; }

        public ObservableCollection<string> Words { get; }

        private bool _isSaveEnabled;

        public bool IsSaveEnabled
        {
            get { return _isSaveEnabled;}
            set
            {
                if (value != _isSaveEnabled)
                {
                    _isSaveEnabled = value;
                    NotifyPropertyChanged(() => IsSaveEnabled);
                }
            }
        }

        #endregion

        public CustomDictionaryWindowViewModel()
        {
            _dictionary = new Dictionary();

            Words = new ObservableCollection<string>();
            _dictionary.GetUserDictionaryWords().OrderBy(x => x).ToList().ForEach(Words.Add);
        }

        /// <summary>
        /// Adds word to list
        /// </summary>
        public void AddWord()
        {
            Words.Add(Word);
            Word = null;
            NotifyPropertyChanged(() => Word);

            IsSaveEnabled = true;
        }

        /// <summary>
        /// Saves list
        /// </summary>
        public void Save()
        {
            _dictionary.AddUserDictionaryWords(Words.ToList().OrderBy(x => x).ToList());
            IsSaveEnabled = false;
        }

        /// <summary>
        /// Deletes selected
        /// </summary>
        /// <param name="selectedItems">Selected Items to delete</param>
        public void Delete(IList selectedItems)
        {
            foreach (string word in selectedItems.Cast<string>().ToList())
            {
                Words.Remove(word);
            }
            IsSaveEnabled = true;
        }

        /// <summary>
        /// Deletes all
        /// </summary>
        public void DeleteAll()
        {
            Words.Clear();
            IsSaveEnabled = true;
        }

        /// <summary>
        /// Imports from file
        /// </summary>
        public void ImportFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "Text Files (.txt)|*.txt",
                Multiselect = false
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!File.Exists(fileDialog.FileName))
                return;

            string[] text = File.ReadAllText(fileDialog.FileName).Split(new[] {"\r\n", "\n", " "}, StringSplitOptions.None);
            text.ToList().ForEach(Words.Add);
        }
    }
}
