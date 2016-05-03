using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Dictionaries
{
    /// <summary>
    /// Dictionary to match words and store tokens
    /// </summary>
    public class Dictionary : IDictionary
    {
        private const string CustomDictionaryPath = "UserDictionary.txt";

        /// <summary>
        /// Creates a dictionary and loads all words from the path provided.
        /// </summary>
        public Dictionary()
        {
            _stopWords = new Node();

            // Words
            _words = new Node();
            _wordsList = new List<string>();
            Properties.Resources.EnglishWords.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None).ToList().ForEach(AddWord);

            // Add User Words
            GetUserDictionaryWords().ToList().ForEach(AddWord);
        }

        #region Custom Dictionary

        /// <summary>
        /// Gets list of custom dictionary
        /// </summary>
        public IEnumerable<string> GetUserDictionaryWords()
        {
            return File.Exists(CustomDictionaryPath) ? File.ReadLines(CustomDictionaryPath) : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Writes/Replaces words in custom dictionary
        /// </summary>
        /// <param name="words"></param>
        public void AddUserDictionaryWords(List<string> words)
        {
            File.WriteAllLines(CustomDictionaryPath, words);
        }

        #endregion

        #region Stop Word

        private readonly Node _stopWords;

        /// <summary>
        /// Adds stop words
        /// </summary>
        public void AddStopWord(bool removeProgrammingStopWord, bool removeEnglishStopWord)
        {
            if (removeProgrammingStopWord)
            {
                Properties.Resources.JavaStopWords.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(AddStopWord);
                Properties.Resources.CPlusPlusStopWords.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(AddStopWord);
                Properties.Resources.CSharpStopWords.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(AddStopWord);
            }

            if (removeEnglishStopWord)
            {
                Properties.Resources.EnglishStopWords.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).ToList().ForEach(AddStopWord);
            }
        }

        /// <summary>
        /// Adds stop word to dictionary
        /// </summary>
        /// <param name="stopWord">Stop Word to be added to the dictionary</param>
        private void AddStopWord(string stopWord)
        {
            Node currentNode = _stopWords;

            stopWord = stopWord.ToLowerInvariant();
            foreach (char termChar in stopWord)
            {
                currentNode = currentNode.GetNode(termChar) ?? currentNode.AddNode(termChar);
            }

            currentNode.IsEnd = true;
        }

        /// <summary>
        /// Return if stop word is part of dictionary
        /// </summary>
        /// <param name="term">term to be checked in dictionary</param>
        /// <returns>true if the specified word is part of dictionary as stop word</returns>
        public bool IsStopWord(string term)
        {
            return IsStopWord(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if stop word is part of dictionary
        /// </summary>
        /// <param name="term">Term as char array to be checked forstop  word</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified word is part of dictionary as stop word</returns>
        public bool IsStopWord(char[] term, int startIndex, int endIndex)
        {
            Node nextNode = _stopWords;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return false;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode != null && nextNode.IsEnd;
        }

        #endregion

        #region Words

        /// <summary>
        /// root of dictionary
        /// </summary>
        private readonly Node _words;

        /// <summary>
        /// string list of words
        /// </summary>
        private readonly List<string> _wordsList;

        /// <summary>
        /// Adds word to dictionary
        /// </summary>
        /// <param name="word">Word to be added to the dictionary</param>
        private void AddWord(string word)
        {
            Node currentNode = _words;

            word = word.ToLowerInvariant();            
            foreach (char termChar in word)
            {
                currentNode = currentNode.GetNode(termChar) ?? currentNode.AddNode(termChar);
            }

            currentNode.IsEnd = true;
            _wordsList.Add(word);
        }

        /// <summary>
        /// Gets list of word
        /// </summary>
        /// <returns>List of all words in dictionary.</returns>
        public IReadOnlyList<string> GetWordList()
        {
            return _wordsList.AsReadOnly();
        }

        /// <summary>
        /// Return if word is part of dictionary
        /// </summary>
        /// <param name="term">term to be checked in dictionary</param>
        /// <returns>true if the specified word is part of dictionary as word</returns>
        public bool IsWord(string term)
        {
            return IsWord(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if word is part of dictionary
        /// </summary>
        /// <param name="term">Term as char array to be checked for word</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified word from start to end index is found in dictionary as word</returns>
        public bool IsWord(char[] term, int startIndex, int endIndex)
        {
            Node nextNode = _words;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return false;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Returns a list of end indexes from start position that will result in an identified term 
        /// </summary>
        /// <param name="term">Term to be searhced in</param>
        /// <param name="startIndex">Start index of term to consider in search</param>
        /// <param name="endIndex">End index of term to be considered in search</param>
        /// <returns>Lists all possible end indexes that will produce left identified</returns>
        public List<SplitPositionWithIdentification> GetPossibleEndIndexesList(char[] term, int startIndex, int endIndex)
        {
            List<SplitPositionWithIdentification> result = new List<SplitPositionWithIdentification>();

            // words
            Node nextNode = _words;
            for (int position = startIndex; position <= endIndex; position++)
            {
                if (nextNode == null)
                {
                    break;
                }

                nextNode = nextNode.GetNode(term[position]);
                if (nextNode != null && nextNode.IsEnd)
                {
                    result.Add(new SplitPositionWithIdentification(position, SplitIdentification.Identified));
                }
            }

            return result;
        }

        #endregion

        #region Word Acronym

        /// <summary>
        /// Checks if a term starts some word in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts word</param>
        /// <returns>Returns true if the term is starting of some word</returns>
        public bool StartsWord(string term)
        {
            return StartsWord(term.ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Checks if a term starts some word in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts word</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>Returns true if the term is starting of some word</returns>
        public bool StartsWord(char[] term, int startIndex, int endIndex)
        {
            Node nextNode = _words;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return false;
                }
                nextNode = nextNode.GetNode(term[i]);
            }

            return nextNode != null;
        }

        /// <summary>
        /// Checks if term expand to some word
        /// </summary>
        /// <param name="term">Term to be checked if it forms a word</param>
        /// <returns>Returns true if the term is part of some word</returns>
        public bool FormsWord(string term)
        {
            return false;
        }

        #endregion
    }
}
