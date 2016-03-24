using System;

namespace SourceCodeIndexer.STAC.TextCorrector
{
    public class LevenshteinDistanceCalculator
    {
        private int?[,] _distance;

        /// <summary>
        /// Calculates distance between two texts
        /// </summary>
        /// <param name="text1">Text 1</param>
        /// <param name="text2">Text 2</param>
        /// <returns>Distance between them</returns>
        public int? CalculateDistance(string text1, string text2)
        {
            _distance = new int?[text1.Length + 1, text2.Length + 1];
            return CalculateDistance(text1, text1.Length, text2, text2.Length);
        }

        private int CalculateDistance(string text1, int text1Length, string text2, int text2Length)
        {
            if (text1Length == 0)
            {
                _distance[0, text2Length] = text2Length;
                return text2Length;
            }

            if (text2Length == 0)
            {
                _distance[text1Length, 0] = text1Length;
                return text1Length;
            }

            if (_distance[text1Length - 1, text2Length] == null)
            {
                _distance[text1Length - 1, text2Length] = CalculateDistance(text1, text1Length - 1, text2, text2Length);
            }

            if (_distance[text1Length, text2Length - 1] == null)
            {
                _distance[text1Length, text2Length - 1] = CalculateDistance(text1, text1Length, text2, text2Length - 1);
            }

            if (_distance[text1Length - 1, text2Length - 1] == null)
            {
                _distance[text1Length - 1, text2Length - 1] = CalculateDistance(text1, text1Length - 1, text2, text2Length - 1);
            }

            int cost = (text1[text1Length - 1] == text2[text2Length - 1]) ? 0 : 1;
            return Math.Min(_distance[text1Length - 1, text2Length].Value + 1, Math.Min(_distance[text1Length, text2Length - 1].Value + 1, _distance[text1Length - 1, text2Length - 1].Value + cost));
        }
    }
}
