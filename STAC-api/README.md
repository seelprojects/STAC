##Using the dll

STAC operates through a single Indexer class located in SourceCodeIndexer.STAC namespace. The constructor for this class takes in an object of type IndexerConfiguration located in SourceCodeIndexer.STAC.Models namespace, which provides all the necessary settings for STAC. Then, the instance method Execute() can be used to execute Indexer and get the result.

####Initializing IndexerConfiguration

An IndexerConfiguration object can be obtained through IndexerConfiguration.GetIndexerConfiguration() method. This method takes in various parameters described below:

#####SplitterSetting
- CustomSplitter: This property can be set with a custom splitter that implements ISplitter interface located in SourceCodeIndexer.STAC.Splitter namespace. If set, this splitter will be used by STAC to split all terms.
- UseDefaultSplitterOfType: This property can be set with SplitterType enum value to use CamelCase splitter or Advance splitting inbuilt in STAC.

If none of the properties are set, STAC will not split any term.

#####DictionarySetting
- CustomDictionary: This property can be set with a custom dictionary that implements IDictionary interface located in SourceCodeIndexer.STAC.Dictionaries namespace. If set, this dictionary will be used to identify natural words.
- DefaultDictionaryRemoveEnglishStopWord: Setting this to true will remove English stop-words from results when using STAC inbuilt dictionary.
- DefaultDictionaryRemoveProgrammingStopWord: Setting this to true will remove programming stop-words from results when using STAC inbuilt dictionary (currently STAC supports c#, java and c++).

If none of the properties are set, STAC will use inbuilt dictionary and will not remove any stop-word.

#####StemmerSetting
- CustomStemmer: This property can be set with a custom stemmer/lemmatizer that implements IStemmer interface located in SourceCodeIndexer.STAC.Stemmer namespace. If set, this stemmer/lemmatizer will be used by STAC to stem/lemmatize all terms.
- UseDefaultPorterStemmer: If set to true, Porter stemmer will be used.

If none of the properties are set, STAC will not stem/lemmatize any term.

#####TextCorrectorSetting
- CustomTextCorrector: This property can be set with a custom text corrector that implements ITextCorrector Interface located in SourceCodeIndexer.STAC.TextCorrector namespace. If set, this text corrector will be used to identify and correct any possible misspelled word found during indexing.
- UseDefaultSpellChecker: If set to true, default text correcter inbuilt in STAC will be used.

If none of the property are set, STAC will not identify or correct misspelled term.

#####TextExtractorSetting
- CustomTextExtractors: This property can be set with list of text extractors that implements ITextExtractor found in SourceCodeIndexer.STAC.TextExtractors namespace. Text extractors are used to extract comments/source code from source code file. Exception will occur if an extension of a source code file being indexed is not handled by any text extractors. By default STAC will index .cs, .cpp, .h and .java files and has inbuilt text extractors for these file types.
- ExtractType: Determines either to extract comments, identifiers and string literals, or both. Default is both.

If none of the properties are set, default extractors will be used.

#####SelectedFiles
This parameter consists a list of file to be indexed.

#####NotificationHandler
This parameter set an object that implements INotificationHandler found in SourceCodeIndexer.STAC.Notification namespace.

```csharp
SplitterSetting splitterSetting = new SplitterSetting();
DictionarySetting dictionarySetting = new DictionarySetting();
TextExtractorSetting textExtractorSetting = new TextExtractorSetting();
TextCorrectorSetting textCorrectorSetting = new TextCorrectorSetting();
StemmerSetting StemmerSetting = new StemmerSetting();
List<IndexerFile> selectedFiles = new List<IndexerFile>();
IndexerConfiguration indexerConfiguration = IndexerConfiguration.GetIndexerConfiguration(splitterSetting, dictionarySetting, textExtractorSetting, textCorrectorSetting, stemmerSetting, selectedFiles, null)
```

####Handling Notification
STAC update notification that indicates files being processed and percentage of process completed. This is done via UpdateStatus() method in notification handler object that implements INotificationHandler interface located in SourceCodeIndexer.STAC.Notification namespace. This object also provides GetYesNoAnswer() method that can be used to receive any exception during indexing a file and decide whether to abort the process or (response from this method is false) just skip the file (response from this method is true, default resonse when no handler is provided). 

####Initializing Indexer
An Indexer object can be created using its constructor that takes in IndexerConfiguration Object.
```csharp
Indexer indexer = new Indexer(indexerConfiguration);
```

####Obtaining Result
IndexerResult object can be obtained by execuing Execute() method of indexer object. The IndexerObject provides following methods:
- **GetCorrectionDictionary()**: returns a list of word, with correction for each word and a list of file where the word is found. If text corrector is not set, returns empty list.
- **GetDictionaryWordList()**: returns list of natural language word with a list of file where the word is found.
- **GetStemmedDictionary()**: returns a list of stemmed word with original word and a list of filewhere the word is found
- **GetTokenList()**: returns a list of abbreviations/system specific words determined by STAC with list of file where the word is found
- **GetUnidentifiedList()**: returns list of unidentified words and list of file where the word is found

####Code Sample

```csharp
using System.Collections.Generic;
using SourceCodeIndexer.STAC;
using SourceCodeIndexer.STAC.Models;

class Program
{
    static void Main(string[] args)
    {
        IndexerConfiguration.SplitterSetting splitterSetting = new IndexerConfiguration.SplitterSetting();
        IndexerConfiguration.DictionarySetting dictionarySetting = new IndexerConfiguration.DictionarySetting();
        IndexerConfiguration.TextExtractorSetting textExtractorSetting = new IndexerConfiguration.TextExtractorSetting();
        IndexerConfiguration.TextCorrectorSetting textCorrectorSetting = new IndexerConfiguration.TextCorrectorSetting();
        IndexerConfiguration.StemmerSetting stemmerSetting = new IndexerConfiguration.StemmerSetting();
        List<IndexerFile> selectedFiles = new List<IndexerFile>();
        IndexerConfiguration indexerConfiguration = IndexerConfiguration.GetIndexerConfiguration(splitterSetting, dictionarySetting, textExtractorSetting, textCorrectorSetting, stemmerSetting, selectedFiles, null);
        Indexer indexer = new Indexer(indexerConfiguration);
        IndexerResult result = indexer.Execute();
    }
}
```
