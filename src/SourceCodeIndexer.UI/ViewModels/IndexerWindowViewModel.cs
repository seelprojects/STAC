using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Configuration;
using System.Windows;
using SourceCodeIndexer.STAC;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.FileStats;
using SourceCodeIndexer.STAC.Models;
using SourceCodeIndexer.STAC.Notification;
using SourceCodeIndexer.STAC.Stemmer;
using SourceCodeIndexer.STAC.TextExtractors;
using SourceCodeIndexer.UI.Enum;
using SourceCodeIndexer.UI.Models;
using MessageBox = System.Windows.MessageBox;
using SplitterType = SourceCodeIndexer.STAC.Enum.SplitterType;

namespace SourceCodeIndexer.UI.ViewModels
{
    public class IndexerWindowViewModel : ViewModelBase, INotificationHandler
    {
        private const int WindowHeightMax = 740;
        private const int WindowHeightMin = 480;

        public IndexerWindowViewModel()
        {
            SplitTypes = SplitTypes = EnumUtility.GetSelectEnumItems<ExtractType>();

            WindowHeight = WindowHeightMin;
        }

        #region Lemmatizer

        private IStemmer _stanfordLemmatizer;

        /// <summary>
        /// Tries initializing Stanford Lemmatizer
        /// </summary>
        /// <returns>True if initialized else false</returns>
        public bool TryInitializeLemmatizer()
        {
            try
            {
                string stanfordModelsPath = ConfigurationManager.AppSettings["StanfordModelsPath"];
                if (_stanfordLemmatizer == null)
                {
                    Status = "Initializing Stanford Lemmatizer";
                    _stanfordLemmatizer = new StanfordLemmatizer(stanfordModelsPath);
                }
                Status = "Stanford Lemmatizer Initialized";
                return true;
            }
            catch (Exception e)
            {
                Status = "Error Initializing Stanford Lemmatizer";
                MessageBox.Show("Stanford Lemmatizer could not be loaded. Exception:" + Environment.NewLine + e.Message, "Error Initializing Stanford Lemmatizer", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion

        #region INotificationHandler

        // This will store result for all. TODO YesToAll for future
        private MessageBoxResult _tempResult = MessageBoxResult.None;
        public bool GetYesNoAnswer(QuestionType questionType, string message)
        {
            if (_tempResult == MessageBoxResult.None)
                _tempResult = MessageBox.Show(message, "Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            return _tempResult == MessageBoxResult.Yes;
        }

        public void UpdateStatus(NotificationType notificationType, int valueCompleted, int totalValue, string message)
        {
            Status = message;
        }

        public void UpdateProgress(double percentCompleted)
        {
            ProgressValue = percentCompleted;
        }

        #endregion

        #region Status/Index

        private bool _hasIndexingResult;
        public bool HasIndexingResult
        {
            get { return _hasIndexingResult;}
            set
            {
                _hasIndexingResult = value;
                NotifyPropertyChanged(() => HasIndexingResult);
                NotifyPropertyChanged(() => DisplayNumberBottom);
                WindowHeight = value ? WindowHeightMax : WindowHeightMin;
            }
        }

        public bool DisplayNumberBottom
        {
            get { return _hasIndexingResult && ! _isIndexingInProgress; }
        }

        private int _windowHeight;
        public int WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                _windowHeight = value;
                NotifyPropertyChanged(() => WindowHeight);
                NotifyPropertyChanged(() => WindowHeightMinResizeable);
            }
        }

        public int WindowHeightMinResizeable
        {
            get { return  _windowHeight > 480 ? _windowHeight - 100 : 480; }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyPropertyChanged(() => ProgressValue);
            }
        }

        private bool _isIndexingInProgress;
        public bool IsIndexingInProgress
        {
            get { return _isIndexingInProgress;}
            set
            {
                _isIndexingInProgress = value;
                NotifyPropertyChanged(() => IsIndexingInProgress);
                NotifyPropertyChanged(() => DisplayNumberBottom);
            }
        }

        private bool _canStartIndexing;
        public bool CanStartIndexing
        {
            get { return _canStartIndexing;}
            set
            {
                _canStartIndexing = value;
                NotifyPropertyChanged(() => CanStartIndexing);
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged(() => Status);
            }
        }

        public bool ApplySpellCheck { get; set; }

        public bool ApplyStemmer { get; set; }

        public bool ApplyLemmatizer { get; set; }

        /// <summary>
        /// Runs indexer
        /// </summary>
        public void IndexFiles()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_WorkCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Runs indexer
        /// </summary>
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            IsIndexingInProgress = true;
            UpdateCanStartIndexing();

            IndexerConfiguration.SplitterSetting splitterSetting = new IndexerConfiguration.SplitterSetting()
            {
                UseDefaultSplitterOfType = ApplyBestSuffixSplit
                    ? SplitterType.Advance
                    : (ApplyCamelCaseSplit ? SplitterType.CamelCase : SplitterType.None),
            };

            IndexerConfiguration.DictionarySetting dictionarySetting = new IndexerConfiguration.DictionarySetting()
            {
                DefaultDictionaryRemoveEnglishStopWord = RemoveEnglishStopWords,
                DefaultDictionaryRemoveProgrammingStopWord = RemoveProgrammingStopWords
            };

            IndexerConfiguration.TextCorrectorSetting textCorrectorSetting = new IndexerConfiguration.
                TextCorrectorSetting()
            {
                UseDefaultSpellChecker = ApplySpellCheck
            };

            ExtractType extractType = 0;
            SplitTypes.Where(x => x.IsSelected).ToList().ForEach(x => extractType |= x.Value);

            IndexerConfiguration.TextExtractorSetting textExtractorSetting = new IndexerConfiguration.
                TextExtractorSetting()
            {
                ExtractType = extractType
            };

            IStemmer stemmer = ApplyLemmatizer ? _stanfordLemmatizer : null;
            IndexerConfiguration.StemmerSetting stemmerSetting = new IndexerConfiguration.StemmerSetting()
            {
                CustomStemmer = stemmer,
                UseDefaultPorterStemmer = ApplyStemmer
            };

            List<IndexerFile> selectedFiles = SelectedFiles.ToList();

            IndexerConfiguration indexerConfiguration = IndexerConfiguration.GetIndexerConfiguration(splitterSetting, dictionarySetting, textExtractorSetting, textCorrectorSetting, stemmerSetting, selectedFiles, this);

            Indexer indexer = new Indexer(indexerConfiguration);
            e.Result = indexer.Execute();
        }

        private IndexerResult _result;

        /// <summary>
        /// Populates result after indexer completes
        /// </summary>
        private void BackgroundWorker_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressValue = 0;
            IsIndexingInProgress = false;
            HasIndexingResult = true;
            NotifyPropertyChanged(() => HasIndexingResult);
            UpdateCanStartIndexing();

            if (e.Error != null)
            {
                Status = e.Error.Message;
                return;
            }

            if (e.Cancelled)
            {
                Status = "Process Cancelled.";
                return;
            }

            IndexerResult result = (IndexerResult) e.Result;
            if (result == null)
            {
                Status = "Process did not return any result.";
                return;
            }

            IdentifierCount = result.GetSplitResultList().Count;
            DictionaryWordsCount = result.GetDictionaryWordList().Count;
            TokensCount = result.GetTokenList().Count;
            UnidentifiedWordsCount = result.GetUnidentifiedList().Count;

            DictionaryWords = string.Join(Environment.NewLine, result.GetDictionaryWordList().Keys.OrderBy(x => x).ThenBy(x => x.Length));
            Tokens = string.Join(Environment.NewLine, result.GetTokenList().Keys.OrderBy(x => x).ThenBy(x => x.Length));
            UnidentifiedWords = string.Join(Environment.NewLine, result.GetUnidentifiedList().Keys.OrderBy(x => x.Length).ThenBy(x => x));
            CorrectedWords = string.Join(Environment.NewLine, result.GetCorrectionDictionary().OrderBy(x => x.Key).Select(x => x.Key + ": " + x.Value.Word));
            StemmedWords = string.Join(Environment.NewLine, result.GetStemmedDictionary().OrderBy(x => x.Key).Select(x => x.Key + ": " + x.Value.Word));

            NotifyPropertyChanged(() => IdentifierCount);
            NotifyPropertyChanged(() => DictionaryWordsCount);
            NotifyPropertyChanged(() => TokensCount);
            NotifyPropertyChanged(() => UnidentifiedWordsCount);
            NotifyPropertyChanged(() => DictionaryWords);
            NotifyPropertyChanged(() => UnidentifiedWords);
            NotifyPropertyChanged(() => Tokens);
            NotifyPropertyChanged(() => StemmedWords);
            NotifyPropertyChanged(() => CorrectedWords);

            _result = result;
            Status = "";
        }

        #endregion

        #region Indexing result

        public int IdentifierCount { get; set; }

        public int DictionaryWordsCount { get; set; }

        public int TokensCount { get; set; }

        public int UnidentifiedWordsCount { get; set; }

        public string DictionaryWords { get; set; }

        public string Tokens { get; set; }

        public string UnidentifiedWords { get; set; }

        public string StemmedWords { get; set; }

        public string CorrectedWords { get; set; }

        #endregion

        #region Splitters

        private bool _applyBestSuffixSplit;

        public bool ApplyBestSuffixSplit
        {
            get { return _applyBestSuffixSplit; }
            set
            {
                _applyBestSuffixSplit = value;
                if (_applyBestSuffixSplit)
                    _applyCamelCaseSplit = false;

                NotifyPropertyChanged(() => ApplyBestSuffixSplit);
                NotifyPropertyChanged(() => ApplyCamelCaseSplit);
            }
        }

        private bool _applyCamelCaseSplit;
        public bool ApplyCamelCaseSplit
        {
            get { return _applyCamelCaseSplit; }
            set
            {
                _applyCamelCaseSplit = value;
                if (_applyCamelCaseSplit)
                    _applyBestSuffixSplit = false;

                NotifyPropertyChanged(() => ApplyBestSuffixSplit);
                NotifyPropertyChanged(() => ApplyCamelCaseSplit);
            }
        }

        public bool RemoveEnglishStopWords { get; set; }

        public bool RemoveProgrammingStopWords { get; set; }

        public List<SelectEnumItem<ExtractType>> SplitTypes { get; }

        #endregion

        #region Text Extractors

        private IList<Tuple<ITextExtractor, FileStatReaderBase>> _textExtractorsWithReaders => new List<Tuple<ITextExtractor, FileStatReaderBase>>()
        {
            new Tuple<ITextExtractor, FileStatReaderBase>(new JavaTextExtractor(), new JavaFileStatReader()),
            new Tuple<ITextExtractor, FileStatReaderBase>(new CPlusPlusTextExtractor(), new CPlusPlusFileStatReader()),
            new Tuple<ITextExtractor, FileStatReaderBase>(new CSharpTextExtractor(), new CSharpFileStatReader())
        };

        #endregion

        #region Files

        public ObservableCollection<IndexerFile> AvailableFiles { get; } = new ObservableCollection<IndexerFile>();

        public ObservableCollection<IndexerFile> SelectedFiles { get; } = new ObservableCollection<IndexerFile>();

        /// <summary>
        /// Moves selected items from available file list to selected file list
        /// </summary>
        /// <param name="selectedItems">Collection of selected items</param>
        internal void MoveSelectedAvailableToSelected(IList selectedItems)
        {
            foreach (IndexerFile file in selectedItems.Cast<IndexerFile>().ToList())
            {
                SelectedFiles.Add(file);
                AvailableFiles.Remove(file);
            }
        }

        /// <summary>
        /// Moves selected items from selected file list to available file list
        /// </summary>
        /// <param name="selectedItems">Collection of selected items</param>
        internal void MoveSelectedSelectedToAvailable(IList selectedItems)
        {
            foreach (IndexerFile file in selectedItems.Cast<IndexerFile>().ToList())
            {
                AvailableFiles.Add(file);
                SelectedFiles.Remove(file);
            }
        }

        /// <summary>
        /// Moves all items from available file list to selected file list
        /// </summary>
        internal void MoveAllAvailableToSelected()
        {
            foreach (IndexerFile file in AvailableFiles)
            {
                SelectedFiles.Add(file);
            }
            AvailableFiles.Clear();
        }

        /// <summary>
        /// Moves all items from selected file list to available file list
        /// </summary>
        internal void MoveAllSelectedToAvailable()
        {
            foreach (IndexerFile file in SelectedFiles)
            {
                AvailableFiles.Add(file);
            }
            SelectedFiles.Clear();
        }

        private ProjectStat _projectStat;

        /// <summary>
        /// Loads Project from path
        /// </summary>
        /// <param name="projectPath">Path to project</param>
        public void LoadFiles(string projectPath)
        {
            SelectedFiles.Clear();
            AvailableFiles.Clear();

            _projectStat = new ProjectStatReader(projectPath, _textExtractorsWithReaders.SelectMany(x => x.Item1.FileExtensionFor()).Distinct().ToList()).GetProjectStat();

            if (_projectStat == null)
            {
                MessageBox.Show("The path: " + projectPath + " does not exists.", "Folder not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // populate list
            _projectStat.FileStats.Select(x => x.IndexerFile).OrderBy(x => x.Name).ToList().ForEach(x => AvailableFiles.Add(x));
            if (_projectStat == null)
                return;

            // populate counts
            Dictionary<string, FileStatReaderBase> extensionFileReaderBases = new Dictionary<string, FileStatReaderBase>();
            foreach (var tuple in _textExtractorsWithReaders)
            {
                foreach (string extension in tuple.Item1.FileExtensionFor())
                {
                    extensionFileReaderBases.Add(extension.ToLowerInvariant(), tuple.Item2);
                }
            }

            foreach (FileStat fileStat in _projectStat.FileStats)
            {
                extensionFileReaderBases[fileStat.IndexerFile.Extension].UpdateFileStatCount(fileStat);
            }

            UpdateCanStartIndexing();
        }

        #endregion

        /// <summary>
        /// Updates start indexing button
        /// </summary>
        public void UpdateCanStartIndexing()
        {
            CanStartIndexing = !IsIndexingInProgress && SplitTypes.Any(x => x.IsSelected) && SelectedFiles.Any();
        }

        #region Menu

        public void DisplayAboutWindow()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        public void DisplayCustomDictionary()
        {
            CustomDictionaryWindow customDictionaryWindow = new CustomDictionaryWindow();
            customDictionaryWindow.ShowDialog();
        }

        public void DisplayLocMetric()
        {
            LocMetricWindow locMetricWindow = new LocMetricWindow();
            locMetricWindow.ViewModel.LoadStats(_projectStat);
            locMetricWindow.ShowDialog();
        }

        public void Export(string exportFolderPath)
        {
            exportFolderPath = exportFolderPath.TrimEnd('\\');
            using (StreamWriter fileWriter = new StreamWriter(exportFolderPath + "\\" + _projectStat.Name + "_file.txt", true))
            {
                SelectedFiles.ToList().ForEach(file =>
                {
                    var allWords = _result.GetDictionaryWordList().Where(x => x.Value.Contains(file)).Select(x => x.Key).OrderBy(x => x).ThenBy(x => x.Length)
                    .Union(_result.GetTokenList().Where(x => x.Value.Contains(file)).Select(x => x.Key).OrderBy(x => x).ThenBy(x => x.Length))
                    .Union(_result.GetUnidentifiedList().Where(x => x.Value.Contains(file)).Select(x => x.Key).OrderBy(x => x.Length).ThenBy(x => x))
                    .Union(_result.GetCorrectionDictionary().Where(x => x.Value.IndexerFiles.Contains(file)).Select(x => x.Key).OrderBy(x => x))
                    .Union(_result.GetStemmedDictionary().OrderBy(x => x.Key).Where(x => x.Value.IndexerFiles.Contains(file)).Select(x => x.Key).OrderBy(x => x));
                    fileWriter.WriteLine(file.Name + " " + string.Join(" ", allWords));
                    fileWriter.Flush();
                });
            }

            File.WriteAllLines(exportFolderPath + "\\" + _projectStat.Name + "_tokens.txt",
                new[] { "Natural Words:" }
                .Union(_result.GetDictionaryWordList().Keys.OrderBy(x => x).ThenBy(x => x.Length))
                .Union(new[] { "Abbreviations:" })
                .Union(_result.GetTokenList().Keys.OrderBy(x => x).ThenBy(x => x.Length))
                .Union(new[] { "Unidentified Words:" })
                .Union(_result.GetUnidentifiedList().Keys.OrderBy(x => x.Length).ThenBy(x => x))
                .Union(new[] { "Spell Checking:" })
                .Union(_result.GetCorrectionDictionary().OrderBy(x => x.Key).Select(x => x.Key + ": " + x.Value.Word))
                .Union(new[] { "Stemmed Words:" })
                .Union(_result.GetStemmedDictionary().OrderBy(x => x.Key).Select(x => x.Key + ": " + x.Value.Word)));

            using (StreamWriter fileWriter = new StreamWriter(exportFolderPath + "\\" + _projectStat.Name + "_split.html", false))
            {
                fileWriter.WriteLine("<!DOCTYPE HTML><html><head><style>span{margin: 5px;}.unidentified{color: red;}.identified{color: black;}.token{color: blue;}.misspelled{color: green}.stemmed{color: #AA3333;}</style></head><body>");
                fileWriter.WriteLine("<h3>Color Index</h3>");
                fileWriter.WriteLine("<span class='identified'>Natural word</span><br />");
                fileWriter.WriteLine("<span class='unidentified'>Unidentified word</span><br />");
                fileWriter.WriteLine("<span class='token'>Abbreviation</span><br />");
                fileWriter.WriteLine("<span class='misspelled'>Misspelled word</span><br />");
                fileWriter.WriteLine("<span class='stemmed'>Stemmed/Lemmatized word</span><br />");
                fileWriter.WriteLine("<br />");
                fileWriter.WriteLine("<h3>Splits</h3>");
                StringBuilder stringBuilder = new StringBuilder();
                _result.GetSplitResultList().ToList().ForEach(identifierSplitResult =>
                {
                    stringBuilder.Append("<div class='");
                    if (identifierSplitResult.Splits.Any(x => x.SplitIdentification == SplitIdentification.Unidentified))
                        stringBuilder.Append("has-unidentified");
                    if (identifierSplitResult.Splits.Any(x => x.SplitIdentification == SplitIdentification.Token || x.SplitIdentification == SplitIdentification.MergedToken))
                        stringBuilder.Append("has-token");
                    if (identifierSplitResult.Splits.Any(x => x.SplitIdentification == SplitIdentification.TokenMisspelled || x.SplitIdentification == SplitIdentification.WordMisspelled))
                        stringBuilder.Append("has-misspelled");
                    stringBuilder.Append("'>" + identifierSplitResult.Identifier + ": ");
                    identifierSplitResult.Splits.ToList().ForEach(split =>
                    {
                        switch (split.SplitIdentification)
                        {
                            case SplitIdentification.Identified:
                                stringBuilder.Append("<span class='identified'>");
                                break;

                            case SplitIdentification.Unidentified:
                                stringBuilder.Append("<span class='unidentified'>");
                                break;

                            case SplitIdentification.MergedToken:
                            case SplitIdentification.Token:
                            case SplitIdentification.SingleLetterIdentifier:
                                stringBuilder.Append("<span class='token'>");
                                break;

                            case SplitIdentification.TokenMisspelled:
                            case SplitIdentification.WordMisspelled:
                                stringBuilder.Append("<span class='misspelled'>");
                                break;

                            case SplitIdentification.WordStemmed:
                            case SplitIdentification.TokenStemmed:
                                stringBuilder.Append("<span class='stemmed'>");
                                break;
                        }
                        stringBuilder.Append(split.Split);
                        stringBuilder.Append("</span>");
                    });
                    stringBuilder.Append("</div>");
                    fileWriter.WriteLine(stringBuilder.ToString());
                    stringBuilder.Clear();
                });

                fileWriter.WriteLine("</body></html>");
            }


        }

        #endregion
    }
}
