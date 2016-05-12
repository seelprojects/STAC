using System;
using System.Collections.Generic;
using System.Linq;
using SourceCodeIndexer.STAC.Dictionaries;
using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.Models
{
    /// <summary>
    /// Holds result for all identifiers in this Indexing process
    /// </summary>
    public class IndexerResult
    {
        /// <summary>
        /// Holds dictionary word list
        /// </summary>
        private readonly Dictionary<string, List<IndexerFile>>  _dictionaryWordList;

        /// <summary>
        /// Holds token list
        /// </summary>
        private readonly Dictionary<string, List<IndexerFile>> _tokenList;

        /// <summary>
        /// Holds unidentified list
        /// </summary>
        private readonly Dictionary<string, List<IndexerFile>> _unidentifiedList;

        /// <summary>
        /// Holds list of stemmed text
        /// </summary>
        private readonly Dictionary<string, WordWithFiles> _stemmedDictionary;

        /// <summary>
        /// Holds list of corrected text
        /// </summary>
        private readonly Dictionary<string, WordWithFiles> _correctedDictionary;

        /// <summary>
        /// Holds list of merged tokens
        /// </summary>
        private readonly Dictionary<string, List<IndexerFile>> _mergedTokenList;

        /// <summary>
        /// Holds actual split result
        /// </summary>
        private readonly List<IdentifierSplitResult> _identifierSplitResults;

        internal IndexerResult()
        {
            _identifierSplitResults = new List<IdentifierSplitResult>();
            _dictionaryWordList = new Dictionary<string, List<IndexerFile>>();
            _tokenList = new Dictionary<string, List<IndexerFile>>();
            _unidentifiedList = new Dictionary<string, List<IndexerFile>>();
            _stemmedDictionary = new Dictionary<string, WordWithFiles>();
            _correctedDictionary = new Dictionary<string, WordWithFiles>();
            _mergedTokenList = new Dictionary<string, List<IndexerFile>>();
        }

        /// <summary>
        /// Adds result to the list.
        /// </summary>
        /// <param name="result">result to be added to the list</param>
        internal void AddSplitResult(IdentifierSplitResult result)
        {
            _identifierSplitResults.Add(result);
            FilterIdentification(result);
        }

        /// <summary>
        /// Gets latest dictionary word list
        /// </summary>
        /// <returns>List of dictionary words</returns>
        public IReadOnlyDictionary<string, List<IndexerFile>> GetDictionaryWordList()
        {
            return _dictionaryWordList;
        }

        /// <summary>
        /// Gets latest tokens list
        /// </summary>
        /// <returns>List of unidentified words</returns>
        public IReadOnlyDictionary<string, List<IndexerFile>> GetUnidentifiedList()
        {
            return _unidentifiedList;
        }

        /// <summary>
        /// Gets latest unidentified list
        /// </summary>
        /// <returns>List of tokens</returns>
        public IReadOnlyDictionary<string, List<IndexerFile>> GetTokenList()
        {
            return _tokenList;
        }

        /// <summary>
        /// Gets split result
        /// </summary>
        /// <returns>List of each identified split result</returns>
        public IReadOnlyList<IdentifierSplitResult> GetSplitResultList()
        {
            return _identifierSplitResults.AsReadOnly();
        }

        /// <summary>
        /// Gets stemmed list. The list is empty if stemming is not applied.
        /// </summary>
        /// <returns>Stemmed list</returns>
        public IReadOnlyDictionary<string, WordWithFiles> GetStemmedDictionary()
        {
            return _stemmedDictionary;
        }

        /// <summary>
        /// Gets correction list. The list is empty if correction is not applied.
        /// </summary>
        /// <returns>Correction list</returns>
        public IReadOnlyDictionary<string, WordWithFiles> GetCorrectionDictionary()
        {
            return _correctedDictionary;
        }

        /// <summary>
        /// Gets Merged Token list.
        /// </summary>
        /// <returns>Merged token list</returns>
        public IReadOnlyDictionary<string, List<IndexerFile>> GetMergedTokenList()
        {
            return _mergedTokenList;
        }

        /// <summary>
        /// Removes stop words
        /// </summary>
        /// <param name="dictionary">Dictionary to use and item less that 2 chars</param>
        internal void RemoveFilterWordAndTokenResult(IDictionary dictionary)
        {
            _dictionaryWordList.Keys.Where(dictionary.IsStopWord).ToList().ForEach(key => _dictionaryWordList.Remove(key));
        }

        /// <summary>
        /// Removes all merged token item and updates respective identified or unidentified
        /// <param name="tokenDictionary">Token dictionary to get result</param>
        /// </summary>
        internal void UpdateFromMergeToken(ITokenDictionary tokenDictionary)
        {
            foreach (KeyValuePair<string, List<IndexerFile>> item in _mergedTokenList)
            {
                SplitWithIdentification splitWithIdentification = tokenDictionary.GetIdentificationSplitForMergedToken(item.Key);
                foreach (IndexerFile indexerFile in item.Value)
                {
                    AddIdentificationToList(splitWithIdentification, indexerFile);
                }
            }
            _mergedTokenList.Clear();
        }

        /// <summary>
        /// Updates misspelled
        /// <param name="tokenDictionary">Token dictionary to get result</param>
        /// </summary>
        internal void UpdateFromMisspelled(ITokenDictionary tokenDictionary)
        {
            foreach (string key in _correctedDictionary.Keys.ToList())
            {
                bool? isToken;
                string correction = tokenDictionary.GetCorrectionForMisspelled(key, out isToken);
                if (!isToken.HasValue)
                    continue;

                _correctedDictionary[key].Word = correction;
                SplitWithIdentification splitWithIdentification = new SplitWithIdentification(correction, isToken.Value ? SplitIdentification.Token : SplitIdentification.Identified);
                foreach (IndexerFile file in _correctedDictionary[key].IndexerFiles)
                {
                    AddIdentificationToList(splitWithIdentification, file);
                }
            }
        }

        /// <summary>
        /// Updates stemmed
        /// <param name="tokenDictionary">Token dictionary to get result</param>
        /// </summary>
        internal void UpdateFromStemmed(ITokenDictionary tokenDictionary)
        {
            foreach (string key in _stemmedDictionary.Keys.ToList())
            {
                bool? isToken;
                string stem = tokenDictionary.GetStemmedForText(key, out isToken);
                if (!isToken.HasValue)
                    continue;

                _stemmedDictionary[key].Word = stem;
                SplitWithIdentification splitWithIdentification = new SplitWithIdentification(stem, isToken.Value ? SplitIdentification.Token : SplitIdentification.Identified);
                foreach (IndexerFile file in _stemmedDictionary[key].IndexerFiles)
                {
                    AddIdentificationToList(splitWithIdentification, file);
                }
            }
        }

        /// <summary>
        /// Replaces identified with new word
        /// </summary>
        /// <param name="identified">Identified in list now</param>
        /// <param name="newIdentified">Text to replace it with</param>
        internal void AddStemmedWordAndReplaceIdentified(string identified, string newIdentified)
        {
            List<IndexerFile> indexerFiles = _dictionaryWordList[identified];
            _dictionaryWordList.Remove(identified);
            if (!_dictionaryWordList.ContainsKey(newIdentified))
            {
                _dictionaryWordList.Add(newIdentified, indexerFiles);
            }
            else
            {
                _dictionaryWordList[newIdentified] = _dictionaryWordList[newIdentified].Union(indexerFiles).Distinct().ToList();
            }

            if (!_stemmedDictionary.ContainsKey(identified))
            {
                _stemmedDictionary.Add(identified, new WordWithFiles(newIdentified, indexerFiles));
            }
        }

        /// <summary>
        /// Creates fresh list of identied, token and unidentified
        /// </summary>
        private void FilterIdentification(IdentifierSplitResult identifierSplitResult)
        {
            identifierSplitResult.Splits.ToList().ForEach(split => AddIdentificationToList(split, identifierSplitResult.IndexerFile));
        }

        /// <summary>
        /// Adds items to respecitive category and associates it with indexer file too
        /// </summary>
        /// <param name="splitWithIdentification">Split information</param>
        /// <param name="file">File the split belongs to</param>
        private void AddIdentificationToList(SplitWithIdentification splitWithIdentification, IndexerFile file)
        {
            string splitToAdd = splitWithIdentification.Split.ToLower();

            switch (splitWithIdentification.SplitIdentification)
            {
                case SplitIdentification.Identified:
                    if (_dictionaryWordList.ContainsKey(splitToAdd))
                    {
                        if (!_dictionaryWordList[splitToAdd].Contains(file))
                        {
                            _dictionaryWordList[splitToAdd].Add(file);
                        }
                    }
                    else
                    {
                        _dictionaryWordList.Add(splitToAdd, new List<IndexerFile>() {file});
                    }
                    break;

                case SplitIdentification.Token:
                case SplitIdentification.SingleLetterIdentifier:
                    if (_tokenList.ContainsKey(splitToAdd))
                    {
                        if (!_tokenList[splitToAdd].Contains(file))
                        {
                            _tokenList[splitToAdd].Add(file);
                        }
                    }
                    else
                    {
                        _tokenList.Add(splitToAdd, new List<IndexerFile>() { file });
                    }
                    break;

                case SplitIdentification.Unidentified:
                    if (_unidentifiedList.ContainsKey(splitToAdd))
                    {
                        if (!_unidentifiedList[splitToAdd].Contains(file))
                        {
                            _unidentifiedList[splitToAdd].Add(file);
                        }
                    }
                    else
                    {
                        _unidentifiedList.Add(splitToAdd, new List<IndexerFile>() { file });
                    }
                    break;

                case SplitIdentification.MergedToken:
                    if (_mergedTokenList.ContainsKey(splitToAdd))
                    {
                        if (!_mergedTokenList[splitToAdd].Contains(file))
                        {
                            _mergedTokenList[splitToAdd].Add(file);
                        }
                    }
                    else
                    {
                        _mergedTokenList.Add(splitToAdd, new List<IndexerFile>() { file });
                    }
                    break;
                case SplitIdentification.WordMisspelled:
                case SplitIdentification.TokenMisspelled:
                    if (!_correctedDictionary.ContainsKey(splitToAdd))
                    {
                        _correctedDictionary.Add(splitToAdd, new WordWithFiles(null, new List<IndexerFile>() { file }));
                    }
                    else
                    {
                        if (!_correctedDictionary[splitToAdd].IndexerFiles.Contains(file))
                        {
                            _correctedDictionary[splitToAdd].IndexerFiles.Add(file);
                        }
                    }
                    break;
                case SplitIdentification.TokenStemmed:
                case SplitIdentification.WordStemmed:
                    if (!_stemmedDictionary.ContainsKey(splitToAdd))
                    {
                        _stemmedDictionary.Add(splitToAdd, new WordWithFiles(null, new List<IndexerFile>() { file }));
                    }
                    else
                    {
                        if (!_stemmedDictionary[splitToAdd].IndexerFiles.Contains(file))
                        {
                            _stemmedDictionary[splitToAdd].IndexerFiles.Add(file);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("SplitIdentification of type " + splitWithIdentification.SplitIdentification + " is not implemented");
            }
        }
    }

    public class WordWithFiles
    {
        public string Word;

        public readonly List<IndexerFile> IndexerFiles;

        public WordWithFiles(string word)
        {
            Word = word;
            IndexerFiles = new List<IndexerFile>();
        }

        public WordWithFiles(string word, List<IndexerFile> indexerFiles)
        {
            Word = word;
            IndexerFiles = indexerFiles;
        }
    }
}
