using System.Collections.Generic;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Dictionaries
{
    public interface ITokenDictionary
    {
        #region Token

        /// <summary>
        /// Adds to token
        /// </summary>
        /// <param name="token">Token to be added</param>
        void AddToken(string token);

        /// <summary>
        /// Return if term is token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified word is token</returns>
        bool IsToken(string term);

        #endregion

        #region Acronym

        /// <summary>
        /// Adds abbreviation
        /// </summary>
        /// <param name="abbreviation">Abbrevation text</param>
        /// <param name="abbreviationFor">Abbreviation for</param>
        void AddAbbreviation(string abbreviation, string abbreviationFor);

        /// <summary>
        /// Return if term is abbreviation for token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified word is abbreviation</returns>
        bool FormsToken(string term);

        /// <summary>
        /// Checks if a term starts some token in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts token</param>
        /// <returns>Returns true if the term is starting of some token</returns>
        bool StartsToken(string term);

        #endregion

        #region End Index

        /// <summary>
        /// Returns a list of end indexes from start position that will result in an identified term
        /// </summary>
        /// <param name="term">Term to be searhced in</param>
        /// <param name="startIndex">Start index of term to consider in search</param>
        /// <param name="endIndex">End index of term to be considered in search</param>
        /// <returns>Lists all possible end indexes that will produce left identified</returns>
        List<SplitPositionWithIdentification> GetPossibleEndIndexesList(char[] term, int startIndex, int endIndex);

        #endregion

        #region Merged Token

        /// <summary>
        /// Adds merged token
        /// </summary>
        /// <param name="mergedToken">Merged text to be added</param>
        /// <param name="identification">The split identification that has primary word for this merge</param>
        void AddMergedToken(string mergedToken, SplitWithIdentification identification);

        /// <summary>
        /// Return if term is merged token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is merged token</returns>
        bool IsMergedToken(string term);
        
        /// <summary>
        /// Get identification split for merged token
        /// </summary>
        /// <param name="mergedToken">Merged Token to get identification</param>
        /// <returns>Split identification</returns>
        SplitWithIdentification GetIdentificationSplitForMergedToken(string mergedToken);

        #endregion

        #region Misspelling

        /// <summary>
        /// Adds misspelled word to dictionary with correction
        /// </summary>
        /// <param name="misspelledWord">misspelled word that is corrected.</param>
        /// <param name="correction">Correction for the misspelled word</param>
        void AddMisspelledWord(string misspelledWord, string correction);

        /// <summary>
        /// Return if term is misspelled word
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is misspelled word</returns>
        bool IsMisspelledWord(string term);

        /// <summary>
        /// Adds misspelled token to dictionary with correction
        /// </summary>
        /// <param name="misspelledToken">misspelled token that is corrected.</param>
        /// <param name="correction">Correction for the misspelled token</param>
        void AddMisspelledToken(string misspelledToken, string correction);

        /// <summary>
        /// Return if term is misspelled token
        /// </summary>
        /// <param name="term">Term as char array to be checked for misspelled token</param>
        /// <returns>true if the specified term is misspelled token</returns>
        bool IsMisspelledToken(string term);
        
        /// <summary>
        /// Return correction for the term
        /// </summary>
        /// <param name="term">Misspelled term for correction</param>
        /// <param name="isToken">Out parameter detemining if misspelled is token or word</param>
        /// <returns>Correction if exists else null</returns>
        string GetCorrectionForMisspelled(string term, out bool? isToken);

        #endregion

        #region Stemmed

        /// <summary>
        /// Adds stemmed term to dictionary with stemmed text
        /// </summary>
        /// <param name="term">term that is stemmed.</param>
        /// <param name="stemmed">Stemmed text for token</param>
        void AddStemmedWord(string term, string stemmed);

        /// <summary>
        /// Return if term is stemmed word
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is stemmed word</returns>
        bool IsStemmedWord(string term);

        /// <summary>
        /// Adds stemmed term to dictionary with stemmed text
        /// </summary>
        /// <param name="term">term that is stemmed.</param>
        /// <param name="stemmed">Stemmed text for token</param>
        void AddStemmedToken(string term, string stemmed);

        /// <summary>
        /// Return if term is stemmed token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is stemmed token</returns>
        bool IsStemmedToken(string term);

        /// <summary>
        /// Return stemmed text for the term
        /// </summary>
        /// <param name="term">Misspelled term for stemmed term</param>
        /// <param name="isToken">Out parameter detemining if stemmed is token or word</param>
        /// <returns>Stemmed if exists else null</returns>
        string GetStemmedForText(string term, out bool? isToken);

        #endregion

        #region Identified

        /// <summary>
        /// Adds dictionary words found in system to token dictionary
        /// </summary>
        /// <param name="word">Dictionary word found in system</param>
        void AddIdentifiedInProject(string word);

        /// <summary>
        /// Gets list of all identified words
        /// </summary>
        /// <returns>List of all identified word</returns>
        IEnumerable<string> GetProjectIdentifiedsAndTokens();

        #endregion
    }
}
