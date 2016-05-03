using System;
using System.ComponentModel;

namespace SourceCodeIndexer.STAC.Enum
{
    /// <summary>
    /// Splitter Type
    /// </summary>
    public enum SplitterType
    {
        None = 0,
        CamelCase,
        Advance
    }

    /// <summary>
    /// Types of split
    /// </summary>
    [Flags]
    public enum ExtractType
    {
        [Name("Index comments")]
        Comments = 1,

        [Name("Index code")]
        IdentifiersAndStringLiterals = 2
    }

    /// <summary>
    /// Split identification
    /// </summary>
    public enum SplitIdentification
    {
        [Name("Unidentified")]
        Unidentified = 1,

        [Name("Token")]
        Token,

        [Name("Identified")]
        Identified,

        [Name("Single Letter Identifier")]
        SingleLetterIdentifier,

        [Name("Merged Token")]
        MergedToken,

        [Name("Token Misspelled")]
        TokenMisspelled,

        [Name("Word Misspelled")]
        WordMisspelled,

        [Name("Stemmed")]
        TokenStemmed,

        [Name("Stemmed")]
        WordStemmed,
    }
}
