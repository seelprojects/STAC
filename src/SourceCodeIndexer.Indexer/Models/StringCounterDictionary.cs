using System.Collections.Generic;
using System.Linq;

namespace SourceCodeIndexer.STAC.Models
{
    internal class StringCounterDictionary
    {
        internal StringCounterDictionary(int count)
        {
            _dictionary = new Dictionary<string, int>();
            _count = count;
        }

        /// <summary>
        /// Dictionary with count
        /// </summary>
        private readonly Dictionary<string, int> _dictionary;

        /// <summary>
        /// Count to make decision
        /// </summary>
        private readonly int _count;

        /// <summary>
        /// Adds item and increase its count
        /// </summary>
        /// <param name="text">string to be added</param>
        internal void Add(string text)
        {
            if (!_dictionary.ContainsKey(text))
            {
                _dictionary.Add(text, 0);
            }
            _dictionary[text] += 1;
        }

        /// <summary>
        /// Returns the list of string with count greater or equal than value supplied
        /// </summary>
        /// <returns>List of string satisfying count</returns>
        internal List<string> GetValidStringListAndRemove()
        {
            List<string> validStringList = _dictionary.Where(x => x.Value >= _count).Select(x => x.Key).ToList();
            validStringList.ForEach(x => _dictionary.Remove(x));
            return validStringList;
        }
    }

    internal class CounterDictionaryWithValue : StringCounterDictionary
    {
        internal CounterDictionaryWithValue(int count)
            : base(count)
        {
            _identificationDictionary = new Dictionary<string, SplitWithIdentification>();
        }

        /// <summary>
        /// Dictionary with identification
        /// </summary>
        private readonly Dictionary<string, SplitWithIdentification> _identificationDictionary;

        /// <summary>
        /// Adds item and increase its count
        /// </summary>
        /// <param name="text">Text to be added</param>
        /// <param name="splitWithIdentification">Corresponding merge</param>
        internal void Add(string text, SplitWithIdentification splitWithIdentification)
        {
            Add(text);
            if (!_identificationDictionary.ContainsKey(text))
            {
                _identificationDictionary.Add(text, splitWithIdentification);
            }
        }

        /// <summary>
        /// Returns the list of string with count greater or equal than value supplied
        /// </summary>
        /// <returns>List of string satisfying count</returns>
        internal List<KeyValuePair<string, SplitWithIdentification>> GetValidItemListAndRemove()
        {
            List<string> validStringList = GetValidStringListAndRemove();
            List<KeyValuePair<string, SplitWithIdentification>> validItemList = validStringList.Select(x => new KeyValuePair<string, SplitWithIdentification>(x, _identificationDictionary[x])).ToList();
            validStringList.ForEach(x => _identificationDictionary.Remove(x));
            return validItemList;
        }
    }
}
