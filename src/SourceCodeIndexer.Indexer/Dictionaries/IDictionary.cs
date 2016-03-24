using System.Collections.Generic;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Dictionaries
{
    public interface IDictionary
    {
        /// <summary>
        /// Return if word is part of dictionary
        /// </summary>
        /// <param name="term">term to be checked in dictionary</param>
        /// <returns>true if the specified word is part of dictionary as word</returns>
        bool IsWord(string term);

        /// <summary>
        /// Return if word is part of dictionary
        /// </summary>
        /// <param name="term">Term as char array to be checked for word</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified word from start to end index is found in dictionary as word</returns>
        bool IsWord(char[] term, int startIndex, int endIndex);

        /// <summary>
        /// Return if stop word is part of dictionary
        /// </summary>
        /// <param name="term">term to be checked in dictionary</param>
        /// <returns>true if the specified word is part of dictionary as stop word</returns>
        bool IsStopWord(string term);

        /// <summary>
        /// Returns a list of end indexes from start position that will result in an identified term 
        /// </summary>
        /// <param name="term">Term to be searhced in</param>
        /// <param name="startIndex">Start index of term to consider in search</param>
        /// <param name="endIndex">End index of term to be considered in search</param>
        /// <returns>Lists all possible end indexes that will produce left identified</returns>
        List<SplitPositionWithIdentification> GetPossibleEndIndexesList(char[] term, int startIndex, int endIndex);

        /// <summary>
        /// Checks if a term starts some word in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts word</param>
        /// <returns>Returns true if the term is starting of some word</returns>
        bool StartsWord(string term);

        /// <summary>
        /// Checks if a term starts some word in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts word</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>Returns true if the term is starting of some word</returns>
        bool StartsWord(char[] term, int startIndex, int endIndex);

        /// <summary>
        /// Checks if term expand to some word
        /// </summary>
        /// <param name="term">Term to be checked if it forms a word</param>
        /// <returns>Returns true if the term is part of some word</returns>
        bool FormsWord(string term);

        /// <summary>
        /// Gets list of word
        /// </summary>
        /// <returns>List of all words in dictionary.</returns>
        IReadOnlyList<string> GetWordList();
    }
}
