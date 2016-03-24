###Using STAC
####Project Settings
<p>This section gives a brief description about the project settings.

######Selecting a project to index

- Click on **Open Project** button or press (ALT + O).
- In *Browse For Folder* dialog, select a project (Java, C# or C++) and click OK.
- Alternatively, a full path for the project folder can also be typed in the textbox followed by the Enter key.


######Selecting files to index

- Select a project to index.
- **Available Files**: Lists all the files in the project. Initially all the project's files are in this list. User can select all <i>Available Files</i> for indexing by clicking >> or select individual files for indexing by clicking >.
- **Selected Files**: Lists all the files to be indexed. User can remove all <i>Selected Files</i> for indexing by clicking << or remove individual selected files by using <.

####Indexer Settings
<p>This section gives a brief description about the indexer settings.

######Selecting splitting technique
STAC splits by default all words based on camel-casing. Advance splitting can be used to split words that are not properly camel-cased. Check **Advance Splitting** checkbox to apply advance splitting. Uncheck the box to apply only camel-case splitting.

######Spell Checking
Incorrect spellings can be checked by checking the **Spell check** checkbox. A list of misspelled words and their corrections is displayed under the "Spell checking" tab once the project is indexed.

######Stemming
STAC uses Porter Stemmer ([http://www.tartarus.org/~martin/PorterStemmer](http://www.tartarus.org/~martin/PorterStemmer)) to reduce words to their morphological roots by removing derivational and inflectional suffixes.
Porter stemmer can be used during indexing by checking the **Stemmer** radio button. A list of words and their roots is displayed under the "Stemming tab" once the project is indexed.

######Lemmatization
STAC provides Stanford coreNLP ([http://stanfordnlp.github.io/CoreNLP/](http://stanfordnlp.github.io/CoreNLP/)) to lemmatize words. 
Lemmatization can be used during indexing by checking the **Lemmatizer** checkbox. A list of words and their roots is displayed under the "Lemmatization tab" once the project is indexed.

####User Dictionary
STAC's maintains a user dictionary to allow users to maintain their own list of words that should not be split or spell-checked during indexing. This feature is accessed from File menu.

- Click on File menu.
- Click **User Dictionary.**
- To add a single word: Type the word in the text box and click **Add Word**. Press **Save** to save the changes.
- To delete a single word: Select the word from the list and click **Delete Selected**. Press **Save** to save the changes.
- To delete all words: Click **Delete All**. Press **Save** to save the changes.
- To import from other text file: User can import list of words from other text file. Click **Import** and select the file to be used.Then press **Open**. Press **Save** to save the changes.


####Saving Result
Result of the indexing process can be saved using STAC's Save menu
        
- Click on File menu.
- Click Save.
- Select a folder where the result text files will be saved and press OK.


####Project Metrics
Project metrics form displays statistical information about the project.

####Setting up the Stanford Lemmatizer

- Download the Stanford Core-NLP (v3.5.2) from [http://stanfordnlp.github.io/CoreNLP/](http://stanfordnlp.github.io/CoreNLP/)) and extract stanford-corenlp-3.5.2-models.jar. Extract the content of the jar file to a folder, let's say x.</TextBlock>
- Open the folder where STAC is installed and locate *STAC.exe.config** 
- Open the file using any text editor. Update the **value** for **StanfordModelsPath** with the folder's path where Stanford models were extracted (here it's path to x folder). For example, if the content of stanford-corenlp-3.5.2-models.jar are extracted to folder x in C:\, the value for StanfordModelsPath will be "C:\x"
- Close and re-run STAC.
