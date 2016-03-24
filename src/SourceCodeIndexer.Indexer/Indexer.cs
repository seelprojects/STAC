using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SourceCodeIndexer.STAC.Dictionaries;
using SourceCodeIndexer.STAC.Models;
using SourceCodeIndexer.STAC.Notification;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC
{
    public class Indexer
    {
        /// <summary>
        /// Configuration setting for indexer to run on
        /// </summary>
        private readonly IndexerConfiguration _configuration;

        /// <summary>
        /// Private token dictionary
        /// </summary>
        private readonly ITokenDictionary _tokenDictionary;

        /// <summary>
        /// Text extractor mapped by file type
        /// </summary>
        private readonly Dictionary<string, ITextExtractor> _textExtractors = new Dictionary<string, ITextExtractor>();

        public Indexer(IndexerConfiguration configuration)
        {
            if (configuration.Dictionary == null)
            {
                throw new InvalidDataException("Error creating Indexer. Dictionary cannot be null");
            }

            if (configuration.TextExtractors == null)
            {
                throw new InvalidDataException("Error creating Indexer. Text Extractor cannot be null");
            }

            _configuration = configuration;

            // wrap the notification handler with our default handler just in case user does not want any notification
            _configuration.NotificationHandler = new NotificationHandler(_configuration.NotificationHandler, _configuration.Stemmer != null);

            // set required stuff for splitter
            _tokenDictionary = new TokenDictionary(_configuration.Dictionary);
            if (_configuration.TextCorrector != null)
            {
                _configuration.TextCorrector.SetTokenDictionary(_tokenDictionary);
                _configuration.TextCorrector.UseDictionary(false);
                _configuration.TextCorrector.UseTokenDictionary(true);
            }

            _configuration.Splitter.SetTextCorrector(_configuration.TextCorrector);
            _configuration.Splitter.SetStemmer(_configuration.Stemmer);
            _configuration.Splitter.SetDictionary(_configuration.Dictionary);
            _configuration.Splitter.SetTokenDictionary(_tokenDictionary);
        }

        #region Process

        /// <summary>
        /// Executes indexer with <see cref="_configuration"/> settings. All notification and results are posted to <see cref="INotificationHandler"/> if set
        /// <returns>Indexer Result</returns>
        /// </summary>
        public IndexerResult Execute()
        {
            RegisterTextExtractors();

            // STEP 1
            // extact all text and update dictionary
            UpdateTokenDictionary();

            // STEP 2
            // extract all identifier and split
            return GetResult();
        }

        /// <summary>
        /// Text extractor for each file type
        /// </summary>
        private void RegisterTextExtractors()
        {
            foreach (ITextExtractor textExtractor in _configuration.TextExtractors)
            {
                foreach (string fileExtension in textExtractor.FileExtensionFor().Select(fileExtensionFor => fileExtensionFor.ToLowerInvariant()))
                {
                    if (_textExtractors.ContainsKey(fileExtension))
                    {
                        throw new InvalidOperationException("Cannot have multiple text extractor for: " + fileExtension + " The extractors are: " + _textExtractors[fileExtension].GetType() + " and " + textExtractor.GetType());
                    }
                    else
                    {
                        _textExtractors.Add(fileExtension, textExtractor);
                    }
                }
            }
        }

        /// <summary>
        /// Updates dictionary
        /// </summary>
        private void UpdateTokenDictionary()
        {
            _configuration.Splitter.SetResultPhase(false);

            // first create a dictionary for tokens
            // extracts all text from source code
            int totalFileCount = _configuration.FilesToScan.Count;
            int currentFileCount = 0;
            foreach (IndexerFile file in _configuration.FilesToScan)
            {
                try
                {
                    _configuration.NotificationHandler.UpdateStatus(NotificationType.AnalyzingFile, currentFileCount, totalFileCount, "Extracting file: " + file.Name);

                    if (!_textExtractors.ContainsKey(file.Extension))
                    {
                        string message = "No extractor is defined for file extension: " + file.Extension + ".  Do you want to skip this file?";
                        if (_configuration.NotificationHandler.GetYesNoAnswer(QuestionType.NoTextExtratorDefined, message))
                        {
                            continue;
                        }
                    }

                    ITextExtractor textExtractor = _textExtractors[file.Extension];
                    string fileText = File.ReadAllText(file.Path);
                    foreach (string identifier in textExtractor.Extract(fileText))
                    {
                        _configuration.NotificationHandler.UpdateStatus(NotificationType.IdentifyingToken, currentFileCount, totalFileCount, "Analyzing token: " + identifier + " in file: " + file.Name);
                        _configuration.Splitter.UpdateTokenDictionary(identifier);
                    }
                }
                catch (Exception e)
                {
                    string additionalMessage = "Error reading file. " + Environment.NewLine + "File: " + file.Name + Environment.NewLine + "Message: " + e.Message + Environment.NewLine + "Do you want to skip this file?";
                    if (!_configuration.NotificationHandler.GetYesNoAnswer(QuestionType.ErrorReadingFile, additionalMessage))
                    {
                        throw;
                    }
                }
                finally
                {
                    currentFileCount++;
                }
            }
        }

        /// <summary>
        /// Get Result
        /// </summary>
        /// <returns>Indexer Result</returns>
        private IndexerResult GetResult()
        {
            IndexerResult indexerResult = new IndexerResult();
            _configuration.Splitter.SetResultPhase(true);

            // extract
            int totalFileCount = _configuration.FilesToScan.Count;
            int currentFileCount = 0;
            foreach (IndexerFile file in _configuration.FilesToScan)
            {
                try
                {
                    _configuration.NotificationHandler.UpdateStatus(NotificationType.ReadingFileForIdentifiers, currentFileCount, totalFileCount, "Extracting file identifier: " + file.Name);

                    if (!_textExtractors.ContainsKey(file.Extension))
                    {
                        string message = "No extractor is defined for file extension: " + file.Extension + ".  Do you want to skip this file?";
                        if (_configuration.NotificationHandler.GetYesNoAnswer(QuestionType.NoTextExtratorDefined, message))
                        {
                            continue;
                        }
                    }

                    ITextExtractor textExtractor = _textExtractors[file.Extension];
                    string fileText = File.ReadAllText(file.Path);
                    foreach (string identifier in textExtractor.Extract(fileText, _configuration.ExtractType))
                    {
                        _configuration.NotificationHandler.UpdateStatus(NotificationType.Splitting, currentFileCount, totalFileCount, "Splitting token: " + identifier + " in file: " + file.Name);
                        IdentifierSplitResult identifierSplitResult = new IdentifierSplitResult(identifier, file);
                        identifierSplitResult.Add(_configuration.Splitter.Split(identifier));
                        indexerResult.AddSplitResult(identifierSplitResult);
                    }
                }
                catch (Exception e)
                {
                    string additionalMessage = "Error reading file. " + Environment.NewLine + "File: " + file.Name +
                                               Environment.NewLine + "Message: " + e.Message + Environment.NewLine +
                                               "Do you want to skip this file?";
                    if (!_configuration.NotificationHandler.GetYesNoAnswer(QuestionType.ErrorReadingFile, additionalMessage))
                    {
                        throw;
                    }
                }
                finally
                {
                    currentFileCount++;
                }
            }

            // Since while adding result we do not have merged token, misspelled and stemmed word info, filter them and add to respective list
            indexerResult.UpdateFromMergeToken(_tokenDictionary);
            indexerResult.UpdateFromMisspelled(_tokenDictionary);
            indexerResult.UpdateFromStemmed(_tokenDictionary);

            // Filter 3: Stem every identified. If the word is identified replace the word with stemmed word
            if (_configuration.Stemmer != null)
            {
                List<string> dictionaryWordList = indexerResult.GetDictionaryWordList().Keys.ToList();
                int totalIdentifiedCount = dictionaryWordList.Count;
                int currentIdentifiedCount = 0;
                foreach (string identified in dictionaryWordList)
                {
                    currentIdentifiedCount++;
                    _configuration.NotificationHandler.UpdateStatus(NotificationType.Stemming, currentIdentifiedCount, totalIdentifiedCount, "Stemming: " + identified);
                    string stemmedText = _configuration.Stemmer.GetStemmedText(identified);
                    if (stemmedText != null && stemmedText != identified && _configuration.Dictionary.IsWord(stemmedText))
                    {
                        indexerResult.AddStemmedWordAndReplaceIdentified(identified, stemmedText);
                    }
                }
            }

            // Filter result
            indexerResult.RemoveFilterWordAndTokenResult(_configuration.Dictionary);

            _configuration.NotificationHandler.UpdateStatus(NotificationType.IndexingCompleted, 1, 1, "Indexing Completed");
            return indexerResult;
        }

        #endregion

    }
}
