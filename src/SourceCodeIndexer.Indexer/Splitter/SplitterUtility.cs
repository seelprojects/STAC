using System;
using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.Splitter
{
    /// <summary>
    /// Maintains splitters
    /// </summary>
    public static class SplitterUtility
    {
        /// <summary>
        /// Gets the score for splitters to decide the splits based on term provided
        /// </summary>
        /// <param name="identificationType">Identification type of split</param>
        /// <returns>Score for identification type</returns>
        internal static int Score(SplitIdentification identificationType)
        {
            switch(identificationType)
            {
                case SplitIdentification.Identified:
                case SplitIdentification.WordMisspelled:
                case SplitIdentification.WordStemmed:
                    return IndexerResources.SplitterIdentifiedScore;

                case SplitIdentification.Token:
                case SplitIdentification.SingleLetterIdentifier:
                case SplitIdentification.MergedToken:
                case SplitIdentification.TokenMisspelled:
                case SplitIdentification.TokenStemmed:
                    return IndexerResources.SplitterTokenScore;

                case SplitIdentification.Unidentified:
                    return IndexerResources.SplitterUnidentifiedScore;

                default:
                    throw new Exception("Not Implemented Score for " + identificationType);
            }
        }

        /// <summary>
        /// Determines if identification is identified type
        /// </summary>
        /// <param name="identification">identification to be checked</param>
        /// <returns>True if identification is token or word</returns>
        internal static bool IsIdentified(SplitIdentification identification)
        {
            return identification == SplitIdentification.Identified;
        }

        /// <summary>
        /// Determines if identification is identified type
        /// </summary>
        /// <param name="identification">identification to be checked</param>
        /// <returns>True if identification is token or word</returns>
        internal static bool IsNotUnidentified(SplitIdentification identification)
        {
            return identification != SplitIdentification.Unidentified && identification != SplitIdentification.SingleLetterIdentifier;
        }

        /// <summary>
        /// Determines if identification is identified type
        /// </summary>
        /// <param name="identification">identification to be checked</param>
        /// <returns>True if identification is token or word</returns>
        internal static bool IsNotUnidentifiedAndNotMerged(SplitIdentification identification)
        {
            return identification != SplitIdentification.Unidentified && identification != SplitIdentification.MergedToken && identification != SplitIdentification.SingleLetterIdentifier;
        }

        /// <summary>
        /// Determines if identification is identified type or token type
        /// </summary>
        /// <param name="identification">identification to be checked</param>
        /// <returns>True if identification is token or word</returns>
        internal static bool IsTokenOrIdentified(SplitIdentification identification)
        {
            return identification == SplitIdentification.Token || identification == SplitIdentification.Identified;
        }

        /// <summary>
        /// Determines if identification is identified type or token type
        /// </summary>
        /// <param name="identification">identification to be checked</param>
        /// <returns>True if identification is token or word</returns>
        internal static bool IsTokenIdentifiedOrMerged(SplitIdentification identification)
        {
            return identification == SplitIdentification.Token || identification == SplitIdentification.Identified || identification == SplitIdentification.MergedToken;
        }

        /// <summary>
        /// Checks if text can be token
        /// </summary>
        /// <param name="text">Text to be checked</param>
        /// <returns>True if the text can be called token</returns>
        internal static bool CanBeToken(string text)
        {
            return text.Length >= IndexerResources.MinTokenLength;
        }
    }
}
