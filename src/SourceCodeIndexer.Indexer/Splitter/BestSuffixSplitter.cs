using System.Collections.Generic;
using System.Linq;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Splitter
{
    public class BestSuffixSplitter : SplitterBase
    {
        public BestSuffixSplitter(bool isPrimarySplitter)
        {
            IsPrimarySplitter = isPrimarySplitter;
        }

        /// <summary>
        /// Returns Display name of splitter
        /// </summary>
        /// <returns>Display Name of splitter</returns>
        public override string GetDisplayame()
        {
            return "Best-Suffix";
        }

        /// <summary>
        /// Current term being splitted
        /// </summary>
        private char[] _term;

        /// <summary>
        /// Hold splits results
        /// </summary>
        private SplitSetScore[] _splitSetScores;

        private SplitterBase _initialSplitter;
        /// <summary>
        /// Gets initial Splitter for for Best Suffix splitter
        /// </summary>
        /// <returns>Initial Splitter of type <see cref="SplitterBase"/> for Best Suffix splitter</returns>
        /// <remarks>Best Suffix splitter uses Camel-Case splitter as initial Splitter</remarks>
        protected override SplitterBase InitialSplitter => _initialSplitter ?? (_initialSplitter = new CamelCaseSplitter(false));

        /// <summary>
        /// Secondary Split - Best Suffix
        /// </summary>
        /// <param name="identifier">Identifier to split</param>
        protected override List<SplitWithIdentification> ApplySplit(string identifier)
        {
            // STEP 1
            // split and identify with initial splitter
            List<SplitWithIdentification> initialSplitResults = null;
            if (InitialSplitter != null)
            {
                initialSplitResults = InitialSplitter.Split(identifier);
            }

            // STEP 2
            // split with primary splitter
            List<SplitWithIdentification> identifierSplitResults = new List<SplitWithIdentification>();
            if (initialSplitResults != null)
            {
                foreach (SplitWithIdentification split in initialSplitResults)
                {
                    if (SplitterUtility.IsNotUnidentified(split.SplitIdentification))
                    {
                        split.Split = split.Split.ToLowerInvariant();
                        identifierSplitResults.Add(split);
                    }
                    else
                    {
                        // Camel case just returns all caps. Check if it can be further splitted into tokens
                        if (IsAllCaps(split.Split) && split.Split.Length >= IndexerResources.MinTokenLengthForCaps)
                        {
                            // Make it lower string so that we wont go over and over it
                            List<SplitWithIdentification> capsSplits = ApplySplit(split.Split.ToLowerInvariant());
                            if (capsSplits.All(capsSplit => SplitterUtility.IsNotUnidentified(capsSplit.SplitIdentification)))
                            {
                                identifierSplitResults.AddRange(capsSplits);
                            }
                            else
                            {
                                identifierSplitResults.Add(split);
                            }
                        }
                        else
                        {
                            identifierSplitResults.AddRange(BestSuffixSplit(split.Split));
                        }
                    }
                }
            }
            else
            {
                identifierSplitResults.AddRange(BestSuffixSplit(identifier));
            }
            return identifierSplitResults;
        }

        /// <summary>
        /// Performs split specific to this splitter
        /// </summary>
        /// <returns>List of splits</returns>
        private List<SplitWithIdentification> BestSuffixSplit(string identifier)
        {
            // find best split for whole word
            _term = identifier.ToLowerInvariant().ToCharArray();
            _splitSetScores = new SplitSetScore[_term.Length];
            SplitSetScore wholeWordSplitSetScore = GetBestSplitSetScore(0);

            return wholeWordSplitSetScore.GetSplitWithIdentifications(_term);
        } 

        /// <summary>
        /// Looks up for Split Set Score if not found calculated it
        /// </summary>
        /// <param name="startIndex">Start index of term to calculate split set score</param>
        /// <returns>SplitSetScore for term starting from index</returns>
        private SplitSetScore GetBestSplitSetScore(int startIndex)
        {
            if (startIndex >= _term.Length)
            {
                return null;
            }
            return _splitSetScores[startIndex] ?? PopulateBestSplitSetScore(startIndex, _term.Length - 1);
        }

        /// <summary>
        /// Updates Score in array and return it
        /// </summary>
        /// <param name="startIndex">Start index of term to calculate split set score</param>
        /// <param name="endIndex">End index of term to calculate split set score</param>
        /// <returns>Split Set Score</returns>
        private SplitSetScore PopulateBestSplitSetScore(int startIndex, int endIndex)
        {
            for (int startPosition = endIndex; startPosition >= startIndex; startPosition--)
            {
                List<SplitPositionWithIdentification> possibleIndexes = TokenDictionary.GetPossibleEndIndexesList(_term, startPosition, endIndex);
                SplitSetScore bestSplitSetScore = null;
                for (int currentSplitPosition = startPosition; currentSplitPosition <= endIndex; currentSplitPosition++)
                {
                    SplitPositionWithIdentification splitPositionWithIdentification = possibleIndexes.FirstOrDefault(x => x.Position == currentSplitPosition) ??
                                                                                      new SplitPositionWithIdentification(currentSplitPosition, SplitIdentification.Unidentified);
                    SplitSetScore splitSetScore = new SplitSetScore(splitPositionWithIdentification, GetBestSplitSetScore(currentSplitPosition + 1), startPosition);
                    if (splitSetScore.IsBetterThan(bestSplitSetScore))
                    {
                        bestSplitSetScore = splitSetScore;
                    }
                }
                _splitSetScores[startPosition] = bestSplitSetScore;
            }
            return _splitSetScores[startIndex];
        }

        internal class SplitSetScore
        {
            private readonly int _startIndex;
            private readonly int _endIndex;

            private int _identifiedSplitCount;
            private int _lettersInWordCount;
            private int _lettersInTokenCount;
            private readonly SplitIdentification _identification = SplitIdentification.Unidentified;

            private int _totalSplitsCount;

            private SplitSetScore _nextSplitSetScore;

            internal SplitSetScore(SplitPositionWithIdentification splitPositionWithIdentification, SplitSetScore nextSplitSetScore, int startIndex)
            {
                _startIndex = startIndex;
                _endIndex = splitPositionWithIdentification.Position;

                if (SplitterUtility.IsNotUnidentified(splitPositionWithIdentification.SplitIdentification))
                {
                    _identification = splitPositionWithIdentification.SplitIdentification;
                    _identifiedSplitCount = 1;
                    int length = _endIndex - _startIndex + 1;
                    if (_identification == SplitIdentification.Identified)
                        _lettersInWordCount = length;
                    else
                        _lettersInTokenCount = length;
                }

                _totalSplitsCount = 1;
                AddScore(nextSplitSetScore);
            }

            private void AddScore(SplitSetScore s1)
            {
                _nextSplitSetScore = s1;

                // we could jump from score to score to get counts, but since the count is checked very often add them to the parent's count
                // startIndex and endIndex will be examined from next score
                if (_nextSplitSetScore == null)
                    return;

                _identifiedSplitCount += _nextSplitSetScore._identifiedSplitCount;
                _lettersInWordCount += _nextSplitSetScore._lettersInWordCount;
                _lettersInTokenCount += _nextSplitSetScore._lettersInTokenCount;
                _totalSplitsCount += _nextSplitSetScore._totalSplitsCount;
            }

            internal bool IsBetterThan(SplitSetScore s1)
            {
                if (s1 == null)
                    return true;

                if ((_lettersInWordCount + _lettersInTokenCount) != (s1._lettersInWordCount + s1._lettersInTokenCount))
                    return (_lettersInWordCount + _lettersInTokenCount) > (s1._lettersInWordCount + s1._lettersInTokenCount);

                if (_lettersInWordCount != s1._lettersInWordCount)
                    return _lettersInWordCount > s1._lettersInWordCount;

                if (_totalSplitsCount != s1._totalSplitsCount)
                    return _totalSplitsCount < s1._totalSplitsCount;

                if (_identifiedSplitCount != s1._identifiedSplitCount)
                    return _identifiedSplitCount > s1._identifiedSplitCount; 

                return true;
            }

            internal List<SplitWithIdentification> GetSplitWithIdentifications(char[] term)
            {
                SplitSetScore currentSplitSetScore = this;
                List<SplitWithIdentification> result = new List<SplitWithIdentification>();

                do
                {
                    SplitWithIdentification splitWithIdentification = new SplitWithIdentification(new string(term, currentSplitSetScore._startIndex, currentSplitSetScore._endIndex - currentSplitSetScore._startIndex + 1), currentSplitSetScore._identification);
                    result.Add(splitWithIdentification);
                    currentSplitSetScore = currentSplitSetScore._nextSplitSetScore;
                } while (currentSplitSetScore != null);

                return result;
            } 
        }
    }
}
