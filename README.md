# STAC
STAC is a tool that supports Static Textual Analysis of Code. It is a light-weight tool that integrates basic features of code indexing into a one-stop stand-alone solution.

###Features

#####Text Extraction
STAC uses regular expressions to recognize the different text patterns in source code. Therefore, the code does not have to
compile, or even to be complete, for STAC to work. Different regular expressions are provided to match the programming language of the project being indexed.

#####Splitting
STAC uses Camel-case splitting to split tokens into natural language word and system specific word (such as abbreviations and acronyms). The user can choose "advance splitting" to split tokens that do not follow proper camel-casing.

#####Stemming
STAC uses Porter Stemmer ([http://www.tartarus.org/~martin/PorterStemmer](http://www.tartarus.org/~martin/PorterStemmer)) to reduce words to their morphological roots by removing derivational and inflectional suffixes.

#####Lemmatization
STAC integrates the Stanford coreNLP ([http://stanfordnlp.github.io/CoreNLP/](http://stanfordnlp.github.io/CoreNLP/)) to reduce words to their lemmas (lingustic valid forms).

#####Other features
- STAC provides a spell-checking feature.
- STAC Generates basic statistical information about the project.
- STAC integrates a user-defined dictionary to allow the user to determine certain tokens that should not be split
- STAC maintains two lists of stop words, including natural language words (e.g., the, shall) and programming keywords (e.g., private, int)

###Language Supported
Currently STAC supports the following programming languages:
- C# (.cs)
- Java (.java)
- C++ (.cpp, .h)

##Getting Started
STAC requires [.Net 4.5.2](https://support.microsoft.com/en-us/kb/2901907) to run. Once downloaded and installed, simply download [exec/](exec/) directory and run STAC.exe.

For more details on using STAC, see [Help.md](Help.md)

###Development Environment
In order to open and modify the C# source project you will need:
- Visual Studio 2015 [Free Community Edition](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx)
- [.Net 4.5.2](https://support.microsoft.com/en-us/kb/2901907)

Once available, open SourceCodeIndexer.sln in [src](src/) directory in Visual Studio and make SourceCodeIndexer.UI as the startup project.
