using System;
using System.Collections.Generic;
using System.Linq;
using SourceCodeIndexer.STAC.Dictionaries;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.Notification;
using SourceCodeIndexer.STAC.Splitter;
using SourceCodeIndexer.STAC.Stemmer;
using SourceCodeIndexer.STAC.TextCorrector;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC.Models
{
    public class IndexerConfiguration
    {
        /// <summary>
        /// Sets Splitter settings
        /// </summary>
        public class SplitterSetting
        {
            /// <summary>
            /// Assigns custom splitter. This splitter will be used
            /// </summary>
            public ISplitter CustomSplitter { get; set; }

            /// <summary>
            /// Uses default splitter based on splitter type chosen.
            /// </summary>
            public SplitterType UseDefaultSplitterOfType { get; set; }
        }

        /// <summary>
        /// Get splitter for splitter type
        /// </summary>
        /// <param name="splitterSetting">Splitter Setting</param>
        /// <returns>Splitter</returns>
        private static ISplitter GetSplitter(SplitterSetting splitterSetting)
        {
            if (splitterSetting.CustomSplitter != null)
                return splitterSetting.CustomSplitter;

            switch (splitterSetting.UseDefaultSplitterOfType)
            {
                case SplitterType.CamelCase:
                    return new CamelCaseSplitter(true);

                case SplitterType.Advance:
                    return new BestSuffixSplitter(true);

                default:
                    return new EmptySplitter();
            }
        }

        /// <summary>
        /// Dictionary setting to be used. Allows user to use custom dictionary
        /// </summary>
        public class DictionarySetting
        {
            /// <summary>
            /// Custom dictionary to be used
            /// </summary>
            public IDictionary CustomDictionary { get; set; }

            /// <summary>
            /// Removes programming stop word if default dictionary is used
            /// </summary>
            public bool DefaultDictionaryRemoveProgrammingStopWord { get; set; }

            /// <summary>
            /// Removes English stop words if default dictionary is used
            /// </summary>
            public bool DefaultDictionaryRemoveEnglishStopWord { get; set; }
        }

        private static IDictionary GetDictionary(DictionarySetting dictionarySetting)
        {
            if (dictionarySetting.CustomDictionary != null)
                return dictionarySetting.CustomDictionary;

            Dictionary dictionary = new Dictionary();
            dictionary.AddStopWord(dictionarySetting.DefaultDictionaryRemoveProgrammingStopWord, dictionarySetting.DefaultDictionaryRemoveEnglishStopWord);
            return dictionary;
        }

        /// <summary>
        /// Text extractor setting to extract code/comments from source code
        /// </summary>
        public class TextExtractorSetting
        {
            /// <summary>
            /// List of custom text extractors
            /// </summary>
            public IList<ITextExtractor> CustomTextExtractors { get; set; }

            /// <summary>
            /// Extract type: comments only, code only or both
            /// </summary>
            public ExtractType ExtractType { get; set; }
        }

        private static IList<ITextExtractor> GetTextExtractors(TextExtractorSetting textExtractorSetting)
        {
            if (textExtractorSetting.CustomTextExtractors != null && textExtractorSetting.CustomTextExtractors.Any())
                return textExtractorSetting.CustomTextExtractors;

            return new List<ITextExtractor>()
            {
                new JavaTextExtractor(),
                new CPlusPlusTextExtractor(),
                new CSharpTextExtractor()
            };
        }

        private static ExtractType GetSplitType(TextExtractorSetting textExtractorSetting)
        {
            return textExtractorSetting.ExtractType == 0 ? ExtractType.IdentifiersAndStringLiterals | ExtractType.Comments : textExtractorSetting.ExtractType;
        }

        /// <summary>
        /// Text corrector settings for spell check
        /// </summary>
        public class TextCorrectorSetting
        {
            /// <summary>
            /// Custom text corrector
            /// </summary>
            public ITextCorrector CustomTextCorrector { get; set; }

            /// <summary>
            /// Use default spell checker
            /// </summary>
            public bool UseDefaultSpellChecker { get; set; }
        }

        private static ITextCorrector GetTextCorrector(TextCorrectorSetting textCorrectorSetting)
        {
            if (textCorrectorSetting.CustomTextCorrector != null)
                return textCorrectorSetting.CustomTextCorrector;

            return textCorrectorSetting.UseDefaultSpellChecker ? new LevenshteinTextCorrector() : null;
        }

        /// <summary>
        /// Stemmer/Lemmatizer setting
        /// </summary>
        public class StemmerSetting
        {
            /// <summary>
            /// Custom Stemmer/Lemmatizer to be used
            /// </summary>
            public IStemmer CustomStemmer { get; set; }

            /// <summary>
            /// Use default proter stemmer to stem
            /// </summary>
            public bool UseDefaultPorterStemmer { get; set; }
        }

        private static IStemmer GetStemmer(StemmerSetting stemmerSetting)
        {
            if (stemmerSetting.CustomStemmer != null)
                return stemmerSetting.CustomStemmer;

            return stemmerSetting.UseDefaultPorterStemmer ? new PorterStemmer() : null;
        }

        /// <summary>
        /// Gets IndexerConfiguration from settings
        /// </summary>
        /// <param name="splitterSetting">Splitter Setting. Cannot be null.</param>
        /// <param name="dictionarySetting">Cannot be null.</param>
        /// <param name="textExtractorSetting">Cannot be null.</param>
        /// <param name="textCorrectorSetting">Cannot be null.</param>
        /// <param name="stemmerSetting">Cannot be null.</param>
        /// <param name="filesToScan">Cannot be null.</param>
        /// <param name="notificationHandler">Notification updater</param>
        /// <returns></returns>
        public static IndexerConfiguration GetIndexerConfiguration(SplitterSetting splitterSetting, DictionarySetting dictionarySetting,
            TextExtractorSetting textExtractorSetting, TextCorrectorSetting textCorrectorSetting, StemmerSetting stemmerSetting,
            List<IndexerFile> filesToScan, INotificationHandler notificationHandler = null)
        {
            if (splitterSetting == null)
            {
                throw new ArgumentNullException(nameof(splitterSetting));
            }

            if (dictionarySetting == null)
            {
                throw new ArgumentNullException(nameof(dictionarySetting));
            }

            if (textExtractorSetting == null)
            {
                throw new ArgumentNullException(nameof(textExtractorSetting));
            }

            if (textCorrectorSetting == null)
            {
                throw new ArgumentNullException(nameof(textCorrectorSetting));
            }

            if (stemmerSetting == null)
            {
                throw new ArgumentNullException(nameof(stemmerSetting));
            }

            if (filesToScan == null)
            {
                throw new ArgumentNullException(nameof(filesToScan));
            }

            IndexerConfiguration indexerConfiguration = new IndexerConfiguration(GetSplitter(splitterSetting), GetSplitType(textExtractorSetting),
                GetDictionary(dictionarySetting), GetTextExtractors(textExtractorSetting), GetTextCorrector(textCorrectorSetting),
                GetStemmer(stemmerSetting), filesToScan, notificationHandler);

            return indexerConfiguration;
        }

        /// <summary>
        /// Indexer Component to use for indexing
        /// </summary>
        /// <param name="splitter">Splitter to be used</param>
        /// <param name="extractType">Split Type: Comment / Source code</param>
        /// <param name="dictionary">Dictionary to be used. Is safe to share among processes.</param>
        /// <param name="textExtractors">Text extractors to be used. Is safe to share among processes.</param>
        /// <param name="textCorrector">Text Corrector to use</param>
        /// <param name="stemmer">Stemmer to be used.</param>
        /// <param name="filesToScan">Files to scan.</param>
        /// <param name="notificationHandler">Notification handler. Use null to not receive any notification. Default action will be used in case of exceptions. Is safe to share among processes.</param>
        private IndexerConfiguration(ISplitter splitter, ExtractType extractType, 
            IDictionary dictionary, IList<ITextExtractor> textExtractors, ITextCorrector textCorrector, IStemmer stemmer,
            List<IndexerFile> filesToScan, INotificationHandler notificationHandler = null)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (splitter == null)
            {
                throw new ArgumentNullException(nameof(splitter), "Splitter cannot be null. Try setting CustomSplitter or set UseDefaultSplitterOfType to some SplitterType.");
            }

            Splitter = splitter;
            ExtractType = extractType;
            Dictionary = dictionary;
            TextExtractors = textExtractors;
            TextCorrector = textCorrector;
            Stemmer = stemmer;

            FilesToScan = filesToScan;

            NotificationHandler = notificationHandler;
        }

        /// <summary>
        /// Determines Splitter to be used
        /// </summary>
        public ISplitter Splitter { get; }

        /// <summary>
        /// Determines ExtractType to be used
        /// </summary>
        public ExtractType ExtractType { get; }

        /// <summary>
        /// Dictionary to use
        /// </summary>
        public IDictionary Dictionary { get; }

        /// <summary>
        /// Text Extractor to use
        /// </summary>
        public IList<ITextExtractor> TextExtractors { get; }

        /// <summary>
        /// Text corrector to use if required
        /// </summary>
        public ITextCorrector TextCorrector { get; }

        /// <summary>
        /// Stemmer to be used
        /// </summary>
        public IStemmer Stemmer { get; }

        /// <summary>
        /// Complete files path to scan
        /// </summary>
        public List<IndexerFile> FilesToScan { get; }

        /// <summary>
        /// Handles the notification. All response, input request during indexing is called from this
        /// </summary>
        public INotificationHandler NotificationHandler { get; set; }
    }
}