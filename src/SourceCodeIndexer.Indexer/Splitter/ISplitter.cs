using System.Collections.Generic;
using SourceCodeIndexer.STAC.Dictionaries;
using SourceCodeIndexer.STAC.Models;
using SourceCodeIndexer.STAC.Stemmer;
using SourceCodeIndexer.STAC.TextCorrector;

namespace SourceCodeIndexer.STAC.Splitter
{
    public interface ISplitter
    {
        /// <summary>
        /// Sets token dictionary
        /// </summary>
        /// <param name="tokenDictionary">Token Dictionary.</param>
        void SetTokenDictionary(ITokenDictionary tokenDictionary);

        /// <summary>
        /// Sets dictionary
        /// </summary>
        /// <param name="dictionary">Dictionary.</param>
        void SetDictionary(IDictionary dictionary);

        /// <summary>
        /// Sets text corrector
        /// </summary>
        /// <param name="textCorrector">Text corrector to be used if selected. This value is optional</param>
        void SetTextCorrector(ITextCorrector textCorrector);

        /// <summary>
        /// Sets stemmer
        /// </summary>
        /// <param name="stemmer">Text stemmer to be used if selected. This value is optional</param>
        void SetStemmer(IStemmer stemmer);

        /// <summary>
        /// Sets result phase. Expected to be false during UpdateTokenDictionary() call so that text correction does not run.
        /// </summary>
        /// <param name="resultPhase">Current result phase</param>
        void SetResultPhase(bool resultPhase);

        /// <summary>
        /// Returns Display name of splitter
        /// </summary>
        /// <returns>Display Name of splitter</returns>
        string GetDisplayame();

        /// <summary>
        /// Updates dictionary with tokens
        /// </summary>
        /// <param name="unsplittedText">String to be splitted and update dictionary.</param>
        void UpdateTokenDictionary(string unsplittedText);

        /// <summary>
        /// Splits the identifier
        /// </summary>
        /// <param name="identifier">Text to split</param>
        /// <returns>List of splitter with identification</returns>
        List<SplitWithIdentification> Split(string identifier);
    }
}
