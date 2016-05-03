using System;

namespace SourceCodeIndexer.STAC.TextCorrector
{
    public class LevenshteinDistanceCalculatorWithLimit
    {
        private readonly int _maxAllowedCost;
        public LevenshteinDistanceCalculatorWithLimit(int maxAllowedCost)
        {
            _maxAllowedCost = maxAllowedCost;
        }

        private int?[,] _distance;

        private char[] _text1;

        private char[] _text2;

        /// <summary>
        /// Calculates distance between two texts
        /// </summary>
        /// <param name="text1">Text 1</param>
        /// <param name="text2">Text 2</param>
        /// <returns>Distance between them</returns>
        public int? CalculateDistance(string text1, string text2)
        {
            if (text1 == text2)
            {
                return 0;
            }

            _text1 = text1.ToCharArray();
            _text2 = text2.ToCharArray();

            _distance = new int?[_text1.Length + 1, _text2.Length + 1];

            //populate the base
            for (int i = 0; i <= _text1.Length; i++)
            {
                _distance[i, 0] = i;
            }

            for (int i = 0; i <= _text2.Length; i++)
            {
                _distance[0, i] = i;
            }

            int counter = 0;
            for (; counter <= Math.Min(text1.Length, text2.Length); counter++)
            {
                int currentDistance = Levenshtein(counter, counter);
                if (currentDistance > _maxAllowedCost)
                {
                    break;
                }
            }

            if (_text1.Length > counter)
            {
                int remain = _text1.Length - counter;
                for (int i = 0; i <= _text2.Length; i++)
                {
                    if ((_distance[counter, i] + remain) <= _maxAllowedCost)
                    {
                        return _distance[counter, i] + remain;
                    }
                }
                return null;
            }

            if (_text2.Length > counter)
            {
                int remain = _text2.Length - counter;
                for (int i = 0; i <= _text1.Length; i++)
                {
                    if ((_distance[i, counter] + remain) <= _maxAllowedCost)
                    {
                        return _distance[i, counter] + remain;
                    }
                }
                return null;
            }

            return _distance[_text1.Length, _text2.Length];
        }

        /// <summary>
        /// Calculates distance. Stops execution if cost goes beyond currentCost.
        /// </summary>
        /// <param name="position1">Row number in distance array to calculate distance</param>
        /// <param name="position2">Row number in distance array to calculate distance</param>
        /// <returns>Distance between text1 and text2 if lesser than current cost.</returns>
        private int Levenshtein(int position1, int position2)
        {
            if (_distance[position1, position2].HasValue)
            {
                return _distance[position1, position2].Value;
            }

            int cost = _text1[position1 - 1] == _text2[position2 - 1] ? 0 : 1;
            _distance[position1, position2]  = Math.Min(Levenshtein(position1 - 1, position2) + 1, Math.Min(Levenshtein(position1, position2 - 1) + 1, Levenshtein(position1 - 1, position2 - 1) + cost));
            return _distance[position1, position2].Value;
        }
    }
}
