using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.Dictionaries;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.Models;
using SourceCodeIndexer.STAC.Stemmer;
using SourceCodeIndexer.STAC.TextCorrector;
using IDictionary = SourceCodeIndexer.STAC.Dictionaries.IDictionary;

namespace SourceCodeIndexer.STAC.Splitter
{
    public abstract class SplitterBase : ISplitter
    {
        #region Properties/Fields/Constructor

        /// <summary>
        /// Determines if split is for getting result or updating dictionary
        /// </summary>
        private bool _resultPhase;

        /// <summary>
        /// Determines if its a primary splitter
        /// </summary>
        protected bool IsPrimarySplitter { get; set; }

        /// <summary>
        /// Holds possible tokens
        /// </summary>
        private readonly StringCounterDictionary _tokenCounterDictionary;

        /// <summary>
        /// Holds possible tokens by merging
        /// </summary>
        private readonly CounterDictionaryWithValue _mergedTokenCounterDictionary;

        /// <summary>
        /// Camel case split
        /// </summary>
        private readonly Regex _camelcaseSplitterOnSymbols;

        /// <summary>
        /// escaped characters
        /// </summary>
        private readonly Regex _escapedCharacterRegex;

        /// <summary>
        /// Checking all caps
        /// </summary>
        private readonly Regex _allCapsRegex;

        /// <summary>
        /// Non alphabet character regex
        /// </summary>
        private readonly Regex _nonWordRegex;

        /// <summary>
        /// Regex for leading and trailing non-alphabet
        /// </summary>
        private readonly Regex _leadingTrailingNonAlphabetRegex;

        protected SplitterBase()
        {
            _tokenCounterDictionary = new StringCounterDictionary(IndexerResources.TokenCount);
            _mergedTokenCounterDictionary = new CounterDictionaryWithValue(IndexerResources.TokenCount);
            _escapedCharacterRegex = new Regex(RegularExpressions.EscapedCharacters);
            _camelcaseSplitterOnSymbols = new Regex(RegularExpressions.RegexCamelcaseSplitterOnSymbols);
            _allCapsRegex = new Regex(RegularExpressions.RegexAllCaps);
            _nonWordRegex = new Regex(RegularExpressions.RegexNonWords);
            _leadingTrailingNonAlphabetRegex = new Regex(RegularExpressions.RegexLeadingAndTrailingNonAlphabet);
        }

        #endregion

        #region Setters

        protected ITokenDictionary TokenDictionary;
        protected IDictionary Dictionary;
        protected ITextCorrector TextCorrector;
        protected IStemmer Stemmer;

        /// <summary>
        /// Sets token dictionary
        /// </summary>
        /// <param name="tokenDictionary">Token Dictionary. Must be set.</param>
        public void SetTokenDictionary(ITokenDictionary tokenDictionary)
        {
            TokenDictionary = tokenDictionary;
            InitialSplitter?.SetTokenDictionary(tokenDictionary);
        }

        /// <summary>
        /// Sets dictionary
        /// </summary>
        /// <param name="dictionary">Dictionary. Must be set.</param>
        public void SetDictionary(IDictionary dictionary)
        {
            Dictionary = dictionary;
            InitialSplitter?.SetDictionary(dictionary);
        }

        /// <summary>
        /// Sets text corrector
        /// </summary>
        /// <param name="textCorrector">Text corrector to be used if selected. This value is optional</param>
        public void SetTextCorrector(ITextCorrector textCorrector)
        {
            TextCorrector = textCorrector;
            InitialSplitter?.SetTextCorrector(textCorrector);
        }

        /// <summary>
        /// Sets stemmer
        /// </summary>
        /// <param name="stemmer">Text stemmer to be used if selected. This value is optional</param>
        public void SetStemmer(IStemmer stemmer)
        {
            Stemmer = stemmer;
            InitialSplitter?.SetStemmer(stemmer);
        }

        /// <summary>
        /// Sets result phase. Expected to be false during UpdateTokenDictionary() call so that text correction does not run.
        /// </summary>
        /// <param name="resultPhase">Current result phase</param>
        public void SetResultPhase(bool resultPhase)
        {
            _resultPhase = resultPhase;
        }

        #endregion

        #region Creating Dictionary

        /// <summary>
        /// Updates dictionary with tokens
        /// </summary>
        /// <param name="unsplittedText">String to be splitted and update dictionary.</param>
        public void UpdateTokenDictionary(string unsplittedText)
        {
            // STEP 1
            // This step splits the identifiers with camel case. The first letter of each split is extracted and added as acronyms.
            // We will be using split only. We don't identify any word in this process.
            List<SplitWithIdentification> camelCaseSplitResults = CamelCaseSplitIdentifications(unsplittedText);
            if (camelCaseSplitResults.Count >= IndexerResources.MinTokenLength)
            {
                string abbr = string.Empty;
                camelCaseSplitResults.ForEach(split => abbr += split.Split.ElementAt(0));
                TokenDictionary.AddAbbreviation(abbr, unsplittedText);
            }

            // STEP 2
            // Split with initial splitter
            List<SplitWithIdentification> initialSplitterIdentifications;
            if (InitialSplitter == null)
            {
                initialSplitterIdentifications = new List<SplitWithIdentification>() { new SplitWithIdentification(unsplittedText, SplitIdentification.Unidentified) };
            }
            else
            {
                if (InitialSplitter is CamelCaseSplitter)
                {
                    initialSplitterIdentifications = camelCaseSplitResults;
                }
                else
                {
                    initialSplitterIdentifications = InitialSplitter.Split(unsplittedText);
                }
            }

            // STEP 3
            // Split with own splitter
            int splitCount = initialSplitterIdentifications.Count;
            for(int splitCounter = 0; splitCounter < splitCount; splitCounter++)
            {
                SplitWithIdentification split = initialSplitterIdentifications.ElementAt(splitCounter);
                string lowerCaseSplit = split.Split.ToLowerInvariant();

                // if identified add to dictionary
                if (SplitterUtility.IsIdentified(split.SplitIdentification))
                {
                    TokenDictionary.AddIdentifiedInProject(lowerCaseSplit);
                    continue;
                }

                // if its identified go to next
                if (SplitterUtility.IsNotUnidentified(split.SplitIdentification))
                {
                    continue;
                }

                // if the length meets minimum requirement, split it further
                if (SplitterUtility.CanBeToken(split.Split))
                {
                    List<SplitWithIdentification> innerPrimarySplit = Split(lowerCaseSplit);

                    // case 1: Its all caps
                    if (IsAllCaps(split.Split))
                    {
                        // split it once to see if it was formed of some identified texts
                        // this could be the case of natural token such as variable name written in all caps
                        if (innerPrimarySplit.All(x => SplitterUtility.IsNotUnidentified(x.SplitIdentification)))
                        {
                            innerPrimarySplit.Where(x => x.SplitIdentification == SplitIdentification.Identified).ToList().ForEach(x => TokenDictionary.AddIdentifiedInProject(x.Split));
                            initialSplitterIdentifications.RemoveAt(splitCounter);
                            initialSplitterIdentifications.InsertRange(splitCounter, innerPrimarySplit);
                            splitCount += innerPrimarySplit.Count - 1;
                            continue;
                        }
                        
                        // if all identified in the split meets minimum requirement, treat this as normal string rather than Capped abbreviation
                        // if it does not since its all caps add to token
                        if (SplitterUtility.CanBeToken(split.Split))
                        {
                            TokenDictionary.AddToken(lowerCaseSplit);
                            continue;
                        }
                    }

                    // acronym
                    if (IsAcronym(lowerCaseSplit))
                    {
                        TokenDictionary.AddToken(lowerCaseSplit);
                        continue;
                    }

                    if (innerPrimarySplit.All(x => SplitterUtility.IsNotUnidentified(x.SplitIdentification)))
                    {
                        var identifieds = innerPrimarySplit.Where(x => x.SplitIdentification == SplitIdentification.Identified).ToList();
                        if (!identifieds.Any() || identifieds.Any(x => x.Split.Length >= IndexerResources.MinTokenLength))
                        {
                            continue;
                        }
                    }

                    if (Stemmer != null && lowerCaseSplit.Length >= IndexerResources.MinMisspelledStemmedLength)
                    {
                        // check if stemming gives any good result
                        string stemmedText = Stemmer.GetStemmedText(lowerCaseSplit);
                        if (stemmedText != null)
                        {
                            SplitIdentification stemmedTextIdentification = GetSplitIdentification(stemmedText);
                            if (SplitterUtility.IsNotUnidentified(stemmedTextIdentification))
                            {
                                if (stemmedTextIdentification == SplitIdentification.Identified)
                                {
                                    initialSplitterIdentifications.ElementAt(splitCounter).SplitIdentification = SplitIdentification.WordStemmed;
                                    TokenDictionary.AddStemmedWord(lowerCaseSplit, stemmedText);
                                }
                                else
                                {
                                    initialSplitterIdentifications.ElementAt(splitCounter).SplitIdentification = SplitIdentification.TokenStemmed;
                                    TokenDictionary.AddStemmedToken(lowerCaseSplit, stemmedText);
                                }
                                continue;
                            }
                        }
                    }

                    if (TextCorrector != null && lowerCaseSplit.Length >= IndexerResources.MinMisspelledStemmedLength)
                    {
                        // when everything fails we could save some computation for next time if the words identified so far can recognize this as misspelled
                        string correctedText = TextCorrector.Correct(lowerCaseSplit);
                        // we WONT be considering misspelled if the corrected term is started or ended with correction. Eg: awish is corrected to wish but it wont be handled.
                        if (correctedText != null && !(lowerCaseSplit.StartsWith(correctedText) || lowerCaseSplit.EndsWith(correctedText)))
                        {
                            SplitIdentification correctedIdentification = GetSplitIdentification(correctedText);
                            if (SplitterUtility.IsNotUnidentified(correctedIdentification))
                            {
                                if (correctedIdentification == SplitIdentification.Identified)
                                {
                                    initialSplitterIdentifications.ElementAt(splitCounter).SplitIdentification = SplitIdentification.WordMisspelled;
                                    TokenDictionary.AddMisspelledWord(lowerCaseSplit, correctedText);
                                }
                                else
                                {
                                    initialSplitterIdentifications.ElementAt(splitCounter).SplitIdentification = SplitIdentification.TokenMisspelled;
                                    TokenDictionary.AddMisspelledToken(lowerCaseSplit, correctedText);
                                }
                            }
                            continue;
                        }
                    }

                    // if any split create an identified
                    if (innerPrimarySplit.Any(x => SplitterUtility.IsNotUnidentified(x.SplitIdentification) && SplitterUtility.CanBeToken(x.Split)))
                    {
                        initialSplitterIdentifications.RemoveAt(splitCounter);
                        initialSplitterIdentifications.InsertRange(splitCounter, innerPrimarySplit);
                        splitCount += innerPrimarySplit.Count - 1;
                        splitCounter--; // evaluate the current split too for oversplit below
                        continue;
                    }

                    // so far no good. Add it as possible token since it can be a token
                    _tokenCounterDictionary.Add(lowerCaseSplit);
                }

                bool merged = false;
                // use merging to prevent oversplit
                if (splitCounter < splitCount - 1 && SplitterUtility.IsTokenOrIdentified(initialSplitterIdentifications[splitCounter + 1].SplitIdentification)
                    && (splitCounter == 0 || initialSplitterIdentifications[splitCounter - 1].SplitIdentification != SplitIdentification.Token))
                {
                    string identifiedText = initialSplitterIdentifications[splitCounter + 1].Split.ToLowerInvariant();
                    string mergedText = initialSplitterIdentifications[splitCounter].Split.ToLowerInvariant() + identifiedText;
                    if (SplitterUtility.CanBeToken(mergedText))
                    {
                        _mergedTokenCounterDictionary.Add(mergedText, new SplitWithIdentification(identifiedText, initialSplitterIdentifications[splitCounter + 1].SplitIdentification));
                        merged = true;
                    }
                }

                if (splitCounter > 0 && SplitterUtility.IsTokenOrIdentified(initialSplitterIdentifications[splitCounter - 1].SplitIdentification)
                    && (splitCounter == (splitCount - 1) || initialSplitterIdentifications[splitCounter + 1].SplitIdentification != SplitIdentification.Token))
                {
                    string identifiedText = initialSplitterIdentifications[splitCounter - 1].Split.ToLowerInvariant();
                    string mergedText = identifiedText + initialSplitterIdentifications[splitCounter].Split.ToLowerInvariant();
                    if (SplitterUtility.CanBeToken(mergedText))
                    {
                        _mergedTokenCounterDictionary.Add(mergedText, new SplitWithIdentification(identifiedText, initialSplitterIdentifications[splitCounter - 1].SplitIdentification));
                        merged = true;
                    }
                }

                if (!merged)
                {
                    if (splitCounter < splitCount - 1 && initialSplitterIdentifications[splitCounter + 1].SplitIdentification == SplitIdentification.Unidentified)
                    {
                        _tokenCounterDictionary.Add(initialSplitterIdentifications[splitCounter].Split.ToLowerInvariant() + initialSplitterIdentifications[splitCounter + 1].Split.ToLowerInvariant());
                    }
                    else if (splitCounter > 0 && initialSplitterIdentifications[splitCounter - 1].SplitIdentification == SplitIdentification.Unidentified)
                    {
                        _tokenCounterDictionary.Add(initialSplitterIdentifications[splitCounter - 1].Split.ToLowerInvariant() + initialSplitterIdentifications[splitCounter].Split.ToLowerInvariant());
                    }
                }
            }

            List<string> tokens = _tokenCounterDictionary.GetValidStringListAndRemove();

            // if tokens can be splitted into word or token add them as merged token. We will only consider one starting with unidentified followed by identified
            tokens.ForEach(token =>
            {
                List<SplitWithIdentification> tokenSplits = Split(token);
                if (tokenSplits.All(split => SplitterUtility.IsNotUnidentified(split.SplitIdentification)))
                    return;

                if (tokenSplits.Count == 2)
                {
                    if (SplitterUtility.IsNotUnidentifiedAndNotMerged((tokenSplits[1].SplitIdentification)))
                    {
                        TokenDictionary.AddMergedToken(token, tokenSplits[1]);
                    }
                    else if (SplitterUtility.IsNotUnidentifiedAndNotMerged((tokenSplits[0].SplitIdentification)))
                    {
                        TokenDictionary.AddMergedToken(token, tokenSplits[0]);
                    }
                }
                else
                {
                    TokenDictionary.AddToken(token);
                }
            });

            List<KeyValuePair<string, SplitWithIdentification>> mergedTokens = _mergedTokenCounterDictionary.GetValidItemListAndRemove();
            mergedTokens.ForEach(x => TokenDictionary.AddMergedToken(x.Key, x.Value));
        }

        #endregion

        #region Identifying Misspelled

        /// <summary>
        /// Identifies misspelled word in the given split
        /// </summary>
        /// <param name="identifierSplitResults">Current split results</param>
        private List<SplitWithIdentification> IdentifyMisspelled(IReadOnlyCollection<SplitWithIdentification> identifierSplitResults)
        {
            // STEP 1:
            // if there is no unidentified in the split we are good to go
            if (identifierSplitResults.All(split => split.SplitIdentification != SplitIdentification.Unidentified))
                return null;

            int splitCount = identifierSplitResults.Count;

            // STEP 2
            // at this point, we have few identified and few non-identified
            // chances are: 
            //      1. A misspelled word is broken into two
            //      2. Composite word one of which is unidentified
            //      3. Composite word one of which is misspelled

            // get index of all misspelled
            List<int> misspelledIntList =
                identifierSplitResults.Select(
                    (split, index) => new { Index = index, Identification = split.SplitIdentification })
                    .Where(x => x.Identification == SplitIdentification.Unidentified)
                    .Select(x => x.Index)
                    .ToList();

            foreach (int misspelledPosition in misspelledIntList)
            {
                int leftLimit = misspelledPosition;
                int rightLimit = misspelledPosition;

                while (leftLimit >= 0 && rightLimit < splitCount)
                {
                    while (leftLimit >= 0)
                    {
                        string text = GetMergedSplitText(identifierSplitResults, leftLimit, rightLimit);
                        if (text.Length < IndexerResources.MinMisspelledStemmedLength)
                        {
                            leftLimit--;
                            continue;
                        }
                        string correction = TextCorrector.Correct(text);

                        if (correction != null)
                        {
                            // also include the right one just in case we find a match with bigger chance
                            bool hasSecondaryCorrection = true;
                            while (hasSecondaryCorrection && leftLimit >= 0 && rightLimit < splitCount)
                            {
                                string textBeforeMerged = GetMergedSplitText(identifierSplitResults, leftLimit, rightLimit);
                                string textWithLeftMerged = leftLimit - 1 >= 0 ? identifierSplitResults.ElementAt(leftLimit-1).Split + textBeforeMerged : null;
                                string textWithRightMerged = rightLimit + 1 < splitCount ? textBeforeMerged + identifierSplitResults.ElementAt(rightLimit + 1).Split : null;
                                string textWithLeftAndRightMerged = leftLimit - 1 >= 0 && rightLimit + 1 < splitCount? textWithLeftMerged + identifierSplitResults.ElementAt(rightLimit + 1).Split : null;

                                string correctionWithLeft = string.IsNullOrWhiteSpace(textWithLeftMerged) ? null : TextCorrector.Correct(textWithLeftMerged);
                                string correctionWithRight = string.IsNullOrWhiteSpace(textWithRightMerged) ? null : TextCorrector.Correct(textWithRightMerged);
                                string correctionWithLeftAndRight = string.IsNullOrWhiteSpace(textWithLeftAndRightMerged) ? null : TextCorrector.Correct(textWithLeftAndRightMerged);

                                if (correctionWithLeftAndRight != null)
                                {
                                    rightLimit++;
                                    leftLimit--;
                                    correction = correctionWithLeftAndRight;
                                }
                                else if (correctionWithLeft != null && correctionWithLeft != textBeforeMerged)
                                {
                                    leftLimit--;
                                    correction = correctionWithLeft;
                                }
                                else if (correctionWithRight != null && correctionWithRight != textBeforeMerged)
                                {
                                    rightLimit++;
                                    correction = correctionWithRight;
                                }
                                else
                                {
                                    hasSecondaryCorrection = false;
                                }
                            }

                            // add misspelled word for future use
                            var result = identifierSplitResults.Select((split, index) => new { Index = index, Split = split})
                                .Where(x => x.Index < leftLimit || x.Index > rightLimit)
                                .Select(x => x.Split).ToList();
                            var correctedText = string.Join("", identifierSplitResults.Select((split, index) => new {Index = index, Split = split})
                                .Where(x => x.Index >= leftLimit && x.Index <= rightLimit)
                                .Select(x => x.Split.Split));
                            SplitIdentification splitIdentification = GetSplitIdentification(correction);
                            result.Insert(leftLimit, new SplitWithIdentification(correction, splitIdentification));
                            if (splitIdentification == SplitIdentification.Identified)
                            {
                                TokenDictionary.AddMisspelledWord(correctedText, correction);
                            }
                            else
                            {
                                TokenDictionary.AddMisspelledToken(correctedText, correction);
                            }
                            return result;
                        }
                        leftLimit--;
                    }
                    rightLimit++;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets string for splitted identifiers 
        /// </summary>
        /// <param name="identifierSplitResults">result used in this process</param>
        /// <param name="startIndex">start index of item to include in result</param>
        /// <param name="endIndex">end index of item</param>
        /// <returns>String that contains merged text of identifier split results split text from start index to endindex inclusive</returns>
        private static string GetMergedSplitText(IEnumerable<SplitWithIdentification> identifierSplitResults, int startIndex, int endIndex)
        {
            return string.Join("", identifierSplitResults
                .Where((split, index) => index >= startIndex && index <= endIndex)
                .Select(split => split.Split)).ToLowerInvariant();
        }

        #endregion

        #region Child Splitter

        /// <summary>
        /// Returns Display name of splitter
        /// </summary>
        /// <returns>Display Name of splitter</returns>
        public abstract string GetDisplayame();

        /// <summary>
        /// Gets initial Splitter for for this splitter
        /// </summary>
        /// <returns>Initial Splitter of type <see cref="SplitterBase"/> for this splitter</returns>
        protected virtual SplitterBase InitialSplitter => null;

        /// <summary>
        /// Split implemented by particular splitter
        /// </summary>
        /// <param name="identifier">Identifier to split</param>
        /// <returns>Identifier Split Result</returns>
        public List<SplitWithIdentification> Split(string identifier)
        {
            if (_nonWordRegex.Replace(identifier, RegularExpressions.StringEmpty).Length < IndexerResources.MinTokenLength)
            {
                SplitWithIdentification returnSplit = Dictionary.IsWord(identifier) 
                    ? new SplitWithIdentification(_nonWordRegex.Replace(identifier, RegularExpressions.StringEmpty), SplitIdentification.Identified) 
                    : new SplitWithIdentification(_nonWordRegex.Replace(identifier, RegularExpressions.StringEmpty), SplitIdentification.SingleLetterIdentifier);
                return new List<SplitWithIdentification>() {returnSplit};
            }
            identifier = _escapedCharacterRegex.Replace(identifier, RegularExpressions.Space);

            List<SplitWithIdentification> splitResult = ApplySplit(identifier);

            // merge the unidentifier until no more merge is found
            // improper casing might have unintended split
            int splitCount = splitResult.Count;
            for (int splitCounter = 0; splitCounter < splitCount; splitCounter++)
            {
                if (splitResult[splitCounter].SplitIdentification != SplitIdentification.Unidentified)
                    continue;

                string mergeLeftText = splitCounter > 0 ? splitResult[splitCounter - 1].Split.ToLowerInvariant() + splitResult[splitCounter].Split.ToLowerInvariant() : null;
                SplitIdentification mergeLeftIdentification = mergeLeftText == null ? SplitIdentification.Unidentified : GetSplitIdentification(mergeLeftText);

                string mergeRightText = splitCounter < splitCount - 1 ? splitResult[splitCounter].Split.ToLowerInvariant() + splitResult[splitCounter + 1].Split.ToLowerInvariant() : null;
                SplitIdentification mergeRightIdentification = mergeRightText == null ? SplitIdentification.Unidentified : GetSplitIdentification(mergeRightText);

                if (SplitterUtility.IsNotUnidentified(mergeRightIdentification))
                {
                    SplitWithIdentification newSplitWithIdentification = new SplitWithIdentification(mergeRightText, mergeRightIdentification);
                    splitResult.RemoveAt(splitCounter + 1);
                    splitResult.RemoveAt(splitCounter);
                    splitResult.Insert(splitCounter, newSplitWithIdentification);
                    splitCount--;
                    continue;
                }
                else if (SplitterUtility.IsNotUnidentified(mergeLeftIdentification))
                {
                    SplitWithIdentification newSplitWithIdentification = new SplitWithIdentification(mergeLeftText, mergeLeftIdentification);
                    splitResult.RemoveAt(splitCounter);
                    splitResult.RemoveAt(splitCounter - 1);
                    splitResult.Insert(splitCounter - 1, newSplitWithIdentification);
                    splitCount--;
                    splitCounter--;
                    continue;
                }

                if (splitCounter < splitCount - 1 && splitResult[splitCounter + 1].SplitIdentification == SplitIdentification.Unidentified)
                {
                    string unidentifiedMerged = splitResult[splitCounter].Split.ToLowerInvariant() + splitResult[splitCounter + 1].Split.ToLowerInvariant();
                    SplitIdentification unidentifiedMergedIdentification = GetSplitIdentification(unidentifiedMerged);
                    if (SplitterUtility.IsNotUnidentified(unidentifiedMergedIdentification))
                    {
                        SplitWithIdentification newSplitWithIdentification = new SplitWithIdentification(unidentifiedMerged, unidentifiedMergedIdentification);
                        splitResult.RemoveAt(splitCounter + 1);
                        splitResult.RemoveAt(splitCounter);
                        splitResult.Insert(splitCounter, newSplitWithIdentification);
                        splitCount--;
                    }
                }
            }

            // Only run spell check when all split are done i.e. is primary splitter
            if (IsPrimarySplitter && TextCorrector != null && _resultPhase && identifier.Length >= IndexerResources.MinMisspelledStemmedLength)
            {
                List<SplitWithIdentification> splitResultWithCorrection = IdentifyMisspelled(splitResult);
                if (splitResultWithCorrection != null)
                {
                    splitResult = splitResultWithCorrection;
                }
            }

            return splitResult;
        }

        protected abstract List<SplitWithIdentification> ApplySplit(string identifier);

        #endregion

        #region Utility

        /// <summary>
        /// Returns if the text is all caps
        /// </summary>
        /// <param name="text">Text to be matched</param>
        /// <returns>True if the text is all caps</returns>
        protected bool IsAllCaps(string text)
        {
            return _allCapsRegex.IsMatch(text);
        }

        /// <summary>
        /// Gets the identification for split
        /// </summary>
        /// <param name="term">term to get score for</param>
        /// <returns><see cref="SplitIdentification"/> for term</returns>
        protected SplitIdentification GetSplitIdentification(string term)
        {
            if (Dictionary.IsWord(term))
            {
                return SplitIdentification.Identified;
            }

            if (TokenDictionary.IsToken(term))
            {
                return SplitIdentification.Token;
            }

            if (TokenDictionary.IsMergedToken(term))
            {
                return SplitIdentification.MergedToken;
            }

            if (TokenDictionary.IsMisspelledWord(term))
            {
                return SplitIdentification.WordMisspelled;
            }

            if (TokenDictionary.IsMisspelledToken(term))
            {
                return SplitIdentification.TokenMisspelled;
            }

            if (TokenDictionary.IsStemmedWord(term))
            {
                return SplitIdentification.WordStemmed;
            }

            if (TokenDictionary.IsStemmedToken(term))
            {
                return SplitIdentification.TokenStemmed;
            }

            return SplitIdentification.Unidentified;
        }

        /// <summary>
        /// Checks if text is acronym
        /// </summary>
        /// <param name="text">Text that is checked for acronym</param>
        /// <returns>true if text is acronym</returns>
        private bool IsAcronym(string text)
        {
            return Dictionary.StartsWord(text) || Dictionary.FormsWord(text) || TokenDictionary.StartsToken(text) || TokenDictionary.FormsToken(text);
        }

        /// <summary>
        /// Performs camel case split
        /// </summary>
        /// <param name="identifier">Identifier to be splitted</param>
        /// <returns>Camel case split result</returns>
        protected List<SplitWithIdentification> CamelCaseSplitIdentifications(string identifier)
        {
            List<SplitWithIdentification> splitResults = new List<SplitWithIdentification>();

            string[] splitsOnSymbols = _camelcaseSplitterOnSymbols.Split(identifier);

            foreach (string symbolSplit in splitsOnSymbols)
            {
                string trailingRemoved = _leadingTrailingNonAlphabetRegex.Replace(symbolSplit, RegularExpressions.StringEmpty);
                if (trailingRemoved.Length == 0)
                    continue;

                // If identifier is single letter just pass it
                if (trailingRemoved.Length == 1)
                {
                    splitResults.Add(new SplitWithIdentification(trailingRemoved, SplitIdentification.SingleLetterIdentifier));
                    continue;
                }

                // If the identifier is all caps just pass it
                if (IsAllCaps(trailingRemoved) && trailingRemoved.Length >= IndexerResources.MinTokenLengthForCaps)
                {
                    SplitIdentification identification = GetSplitIdentification(trailingRemoved.ToLowerInvariant());
                    splitResults.Add(new SplitWithIdentification(trailingRemoved, identification));
                    continue;
                }

                foreach (string splitsOnTransition in Regex.Split(symbolSplit, RegularExpressions.RegexCamelcaseSplitterOnTransition))
                { 
                    string split = splitsOnTransition.Trim();
                    if (split == "")
                        continue;

                    splitResults.Add(new SplitWithIdentification(split, GetSplitIdentification(split.ToLowerInvariant())));
                }
            }

            return splitResults;
        }

        #endregion
    }
}
