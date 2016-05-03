using System;
using System.Collections.Generic;
using System.Linq;
using SourceCodeIndexer.STAC.Dictionaries;

namespace SourceCodeIndexer.STAC.TextCorrector
{
    public class LevenshteinTextCorrector : ITextCorrector
    {
        public LevenshteinTextCorrector()
        {
            _distanceCalculator = new LevenshteinDistanceCalculator();
        }

        private IDictionary _dictionary;
        private ITokenDictionary _tokenDictionary;

        private bool _useDictionary;
        private bool _useTokenDictionary;

        private readonly LevenshteinDistanceCalculator _distanceCalculator;

        /// <summary>
        /// Sets dictionary to be used
        /// </summary>
        /// <param name="dictionary">Dictionary to be used. Requiered if use dictionary is set to true.</param>
        public void SetDictionary(IDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// Enables/Disables use of dictionary.
        /// </summary>
        /// <param name="useDictionary">True if dictionary is to be used. Must set dictionary if true.</param>
        public void UseDictionary(bool useDictionary)
        {
            _useDictionary = useDictionary;
        }

        /// <summary>
        /// Sets token dictionary to be used
        /// </summary>
        /// <param name="tokenDictionary">Token dictionary to be used. Requiered if use token dictionary is set to true.</param>
        public void SetTokenDictionary(ITokenDictionary tokenDictionary)
        {
            _tokenDictionary = tokenDictionary;
        }

        /// <summary>
        /// Enables/Disables use of token dictionary.
        /// </summary>
        /// <param name="useTokenDictionary">True if token dictionary is to be used. Must set token dictionary if true.</param>
        public void UseTokenDictionary(bool useTokenDictionary)
        {
            _useTokenDictionary = useTokenDictionary;
        }

        /// <summary>
        /// Gets corrected form of the text
        /// </summary>
        /// <param name="text">Text to be corrected</param>
        /// <returns>Corrected text. Null is returned if no match is found.</returns>
        public string Correct(string text)
        {
            CorrectionResult result = new CorrectionResult(null, TextCorrectorResources.MaxDistance + 1);
            int minDictionaryLength = Math.Max(text.Length - TextCorrectorResources.MaxDistance, 1);
            int maxDictionaryLength = text.Length + TextCorrectorResources.MaxDistance;

            if (_useDictionary)
            {
                CorrectionResult dictionaryCorrectionResult = GetBestDistance(text, _dictionary.GetWordList().Where(x => x.Length <= maxDictionaryLength && x.Length >= minDictionaryLength));
                if (dictionaryCorrectionResult.IsBetterThan(result))
                {
                    result = dictionaryCorrectionResult;
                }
            }

            if (_useTokenDictionary)
            {
                CorrectionResult dictionaryCorrectionResult = GetBestDistance(text, _tokenDictionary.GetProjectIdentifiedsAndTokens().Where(x => x.Length <= maxDictionaryLength && x.Length >= minDictionaryLength));
                if (dictionaryCorrectionResult.IsBetterThan(result))
                {
                    result = dictionaryCorrectionResult;
                }
            }

            return result.Text;
        }

        /// <summary>
        /// Gets best distance for given word from given list of words
        /// </summary>
        /// <param name="text">Text to find next best match</param>
        /// <param name="dictionary">Dictionary word</param>
        /// <returns>Best match with count</returns>
        private CorrectionResult GetBestDistance(string text, IEnumerable<string> dictionary)
        {
            int bestDistance = int.MaxValue;
            string bestMatch = null;

            foreach (string dictionaryWord in dictionary)
            {
                int? currentDistance = _distanceCalculator.CalculateDistance(text, dictionaryWord);
                if (currentDistance.HasValue && currentDistance < bestDistance)
                {
                    bestDistance = currentDistance.Value;
                    bestMatch = dictionaryWord;
                }

                if (bestDistance <= 1)
                {
                    //break;
                }
            }
            return new CorrectionResult(bestMatch, bestDistance);
        }
    }

    internal class CorrectionResult
    {
        internal readonly string Text;
        internal readonly int Distance;

        internal CorrectionResult(string text, int distance)
        {
            Text = text;
            Distance = distance;
        }

        internal bool IsBetterThan(CorrectionResult result)
        {
            return Distance < result.Distance;
        }
    }
}
