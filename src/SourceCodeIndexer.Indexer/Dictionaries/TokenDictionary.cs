using System.Collections.Generic;
using System.Linq;
using SourceCodeIndexer.STAC.Enum;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Dictionaries
{
    internal class TokenDictionary : ITokenDictionary
    {
        private readonly IDictionary _dictionary; 

        public TokenDictionary(IDictionary dictionary)
        {
            // Dictionary
            _dictionary = dictionary;
            _identifiedList = new List<string>();

            // Tokens
            _tokens = new Node();

            // Abbreviation
            _acronymDictionary = new NodeWithValue<List<string>>();

            // Merged token
            _mergedTokenDictionary = new NodeWithValue<SplitWithIdentification>();

            // Misspellings
            _misspelledWordDictionary = new NodeWithValue<string> ();
            _misspelledTokenDictionary = new NodeWithValue<string>();

            // Stemmed
            _stemmedWordDictionary = new NodeWithValue<string>();
            _stemmedTokenDictionary = new NodeWithValue<string>();
        }

        #region Token

        private readonly Node _tokens;

        /// <summary>
        /// Adds to token
        /// </summary>
        /// <param name="token">Token to be added</param>
        public void AddToken(string token)
        {
            Node currentNode = _tokens;
            token = token.ToLowerInvariant();

            foreach (char termChar in token)
            {
                Node charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }

            currentNode.IsEnd = true;
        }

        /// <summary>
        /// Return if term is token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified word is token</returns>
        public bool IsToken(string term)
        {
            return IsToken(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is token
        /// </summary>
        /// <param name="term">Term as char array to be checked for token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term from start to end index is token</returns>
        private bool IsToken(char[] term, int startIndex, int endIndex)
        {
            Node nextNode = _tokens;
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

        #region Acronym

        private readonly NodeWithValue<List<string>>  _acronymDictionary;

        /// <summary>
        /// Adds abbreviation
        /// </summary>
        /// <param name="abbreviation">Abbrevation text</param>
        /// <param name="abbreviationFor">Abbreviation for</param>
        public void AddAbbreviation(string abbreviation, string abbreviationFor)
        {
            var currentNode = _acronymDictionary;
            abbreviation = abbreviation.ToLowerInvariant();

            foreach (char termChar in abbreviation)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }

            currentNode.IsEnd = true;
            if (currentNode.Value == null)
            {
                currentNode.Value = new List<string>();
            }
            currentNode.Value.Add(abbreviationFor);
        }

        /// <summary>
        /// Return if term is abbreviation for token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified word is abbreviation</returns>
        public bool FormsToken(string term)
        {
            return FormsToken(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is abbreviation for token
        /// </summary>
        /// <param name="term">Term as char array to be checked for abbreviation of token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term from start to end index is found in dictionary as token</returns>
        private bool FormsToken(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _acronymDictionary;
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
        /// Checks if a term starts some token in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts token</param>
        /// <returns>Returns true if the term is starting of some token</returns>
        public bool StartsToken(string term)
        {
            return StartsToken(term.ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Checks if a term starts some token in dictionary
        /// </summary>
        /// <param name="term">Term to be checked in dictionary that starts token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>Returns true if the term is starting of some token</returns>
        private bool StartsToken(char[] term, int startIndex, int endIndex)
        {
            Node nextNode = _tokens;
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

        #endregion

        #region End Index

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
            result.AddRange(_dictionary.GetPossibleEndIndexesList(term, startIndex, endIndex));

            // tokens
            Node nextNode = _tokens;
            for (int position = startIndex; position <= endIndex; position++)
            {
                if (nextNode == null)
                {
                    break;
                }

                nextNode = nextNode.GetNode(term[position]);
                if (nextNode != null && nextNode.IsEnd)
                {
                    result.Add(new SplitPositionWithIdentification(position, SplitIdentification.Token));
                }
            }

            // misspelled and stemmed
            result.AddRange(GetPossibleEndIndexesList(_misspelledWordDictionary, term, startIndex, endIndex, SplitIdentification.WordMisspelled).Where(x => !result.Select(y => y.Position).Contains(x.Position)));
            result.AddRange(GetPossibleEndIndexesList(_misspelledTokenDictionary, term, startIndex, endIndex, SplitIdentification.TokenMisspelled).Where(x => !result.Select(y => y.Position).Contains(x.Position)));
            result.AddRange(GetPossibleEndIndexesList(_stemmedWordDictionary, term, startIndex, endIndex, SplitIdentification.WordStemmed).Where(x => !result.Select(y => y.Position).Contains(x.Position)));
            result.AddRange(GetPossibleEndIndexesList(_stemmedTokenDictionary, term, startIndex, endIndex, SplitIdentification.TokenStemmed).Where(x => !result.Select(y => y.Position).Contains(x.Position)));
            result.AddRange(GetPossibleEndIndexesList(_acronymDictionary, term, startIndex, endIndex, SplitIdentification.Token).Where(x => !result.Select(y => y.Position).Contains(x.Position)));
            result.AddRange(GetPossibleEndIndexesList(_mergedTokenDictionary, term, startIndex, endIndex, SplitIdentification.MergedToken).Where(x => !result.Select(y => y.Position).Contains(x.Position)));

            return result;
        }

        /// <summary>
        /// List possible indexes for given dictionary
        /// </summary>
        private List<SplitPositionWithIdentification> GetPossibleEndIndexesList<T>(NodeWithValue<T> dictionary, char[] term, int startIndex, int endIndex, SplitIdentification splitIdentification)
        {
            List<SplitPositionWithIdentification> result = new List<SplitPositionWithIdentification>();

            var nextNode = dictionary;
            for (int position = startIndex; position <= endIndex; position++)
            {
                if (nextNode == null)
                {
                    break;
                }

                nextNode = nextNode.GetNode(term[position]);
                if (nextNode != null && nextNode.IsEnd)
                {
                    result.Add(new SplitPositionWithIdentification(position, splitIdentification));
                }
            }

            return result;
        }

        #endregion

        #region Merged Token

        private readonly NodeWithValue<SplitWithIdentification> _mergedTokenDictionary;

        /// <summary>
        /// Adds merged token
        /// </summary>
        /// <param name="mergedToken">Merged text to be added</param>
        /// <param name="identification">The split identification that has primary word for this merge</param>
        public void AddMergedToken(string mergedToken, SplitWithIdentification identification)
        {
            var currentNode = _mergedTokenDictionary;
            mergedToken = mergedToken.ToLowerInvariant();

            foreach (char termChar in mergedToken)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }
            currentNode.IsEnd = true;
            currentNode.Value = identification;
        }

        /// <summary>
        /// Return if term is merged token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is merged token</returns>
        public bool IsMergedToken(string term)
        {
            return IsMergedToken(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is merged token
        /// </summary>
        /// <param name="term">Term as char array to be checked for merged token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term is merged token</returns>
        private bool IsMergedToken(char[] term, int startIndex, int endIndex)
        {
            var nextNode = GetMergedTokenLastNode(term, startIndex, endIndex);
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Get identification split for merged token
        /// </summary>
        /// <param name="mergedToken">Merged Token to get identification</param>
        /// <returns>Split identification</returns>
        public SplitWithIdentification GetIdentificationSplitForMergedToken(string mergedToken)
        {
            var nextNode = GetMergedTokenLastNode(mergedToken);
            return nextNode != null && nextNode.IsEnd ? nextNode.Value : null;
        }

        /// <summary>
        /// Gets last node for the merged token term
        /// </summary>
        private NodeWithValue<SplitWithIdentification> GetMergedTokenLastNode(string term)
        {
            return GetMergedTokenLastNode(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Gets last node for the merged token term
        /// </summary>
        private NodeWithValue<SplitWithIdentification> GetMergedTokenLastNode(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _mergedTokenDictionary;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return null;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode;
        }

        #endregion

        #region Misspelling

        private readonly NodeWithValue<string> _misspelledWordDictionary;

        /// <summary>
        /// Adds misspelled word to dictionary with correction
        /// </summary>
        /// <param name="misspelledWord">misspelled word that is corrected.</param>
        /// <param name="correction">Correction for the misspelled word</param>
        public void AddMisspelledWord(string misspelledWord, string correction)
        {
            var currentNode = _misspelledWordDictionary;
            misspelledWord = misspelledWord.ToLowerInvariant();

            foreach (char termChar in misspelledWord)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }
            currentNode.IsEnd = true;
            currentNode.Value = correction;
        }

        /// <summary>
        /// Return if term is misspelled word
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is misspelled word</returns>
        public bool IsMisspelledWord(string term)
        {
            return IsMisspelledWord(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is misspelled word
        /// </summary>
        /// <param name="term">Term as char array to be checked for token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term is misspelled word</returns>
        private bool IsMisspelledWord(char[] term, int startIndex, int endIndex)
        {
            var nextNode = GetMisspelledWordLastNode(term, startIndex, endIndex);
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Gets misspelled word term last node
        /// </summary>
        private NodeWithValue<string> GetMisspelledWordLastNode(string term)
        {
            return GetMisspelledWordLastNode(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Gets misspelled word term last node
        /// </summary>
        private NodeWithValue<string> GetMisspelledWordLastNode(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _misspelledWordDictionary;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return null;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode;
        }

        private readonly NodeWithValue<string> _misspelledTokenDictionary;

        /// <summary>
        /// Adds misspelled token to dictionary with correction
        /// </summary>
        /// <param name="misspelledToken">misspelled token that is corrected.</param>
        /// <param name="correction">Correction for the misspelled token</param>
        public void AddMisspelledToken(string misspelledToken, string correction)
        {
            var currentNode = _misspelledTokenDictionary;
            misspelledToken = misspelledToken.ToLowerInvariant();

            foreach (char termChar in misspelledToken)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }
            currentNode.IsEnd = true;
            currentNode.Value = correction;
        }

        /// <summary>
        /// Return if term is misspelled token
        /// </summary>
        /// <param name="term">Term as char array to be checked for misspelled token</param>
        /// <returns>true if the specified term is misspelled token</returns>
        public bool IsMisspelledToken(string term)
        {
            return IsMisspelledToken(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is misspelled token
        /// </summary>
        /// <param name="term">Term as char array to be checked for misspelled token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term is misspelled token</returns>
        private bool IsMisspelledToken(char[] term, int startIndex, int endIndex)
        {
            var nextNode = GetMisspelledTokenLastNode(term, startIndex, endIndex);
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Gets misspelled token term last node
        /// </summary>
        private NodeWithValue<string> GetMisspelledTokenLastNode(string term)
        {
            return GetMisspelledTokenLastNode(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Gets misspelled token term last node
        /// </summary>
        private NodeWithValue<string> GetMisspelledTokenLastNode(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _misspelledTokenDictionary;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return null;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode;
        }

        /// <summary>
        /// Return correction for the term
        /// </summary>
        /// <param name="term">Misspelled term for correction</param>
        /// <param name="isToken">Out parameter detemining if misspelled is token or word</param>
        /// <returns>Correction if exists else null</returns>
        public string GetCorrectionForMisspelled(string term, out bool? isToken)
        {
            var misspelledToken = GetMisspelledTokenLastNode(term);
            if (misspelledToken != null && misspelledToken.IsEnd)
            {
                isToken = true;
                return misspelledToken.Value;
            }


            var misspelledWord = GetMisspelledWordLastNode(term);
            if (misspelledWord != null && misspelledWord.IsEnd)
            {
                isToken = false;
                return misspelledWord.Value;
            }

            isToken = null;
            return null;
        }

        #endregion

        #region Stemmed

        private readonly NodeWithValue<string> _stemmedWordDictionary;

        /// <summary>
        /// Adds stemmed term to dictionary with stemmed text
        /// </summary>
        /// <param name="term">term that is stemmed.</param>
        /// <param name="stemmed">Stemmed text for token</param>
        public void AddStemmedWord(string term, string stemmed)
        {
            var currentNode = _stemmedWordDictionary;
            term = term.ToLowerInvariant();

            foreach (char termChar in term)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }
            currentNode.IsEnd = true;
            currentNode.Value = stemmed;
        }

        
        /// <summary>
        /// Return if term is stemmed word
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is stemmed word</returns>
        public bool IsStemmedWord(string term)
        {
            return IsStemmedWord(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is stemmed word
        /// </summary>
        /// <param name="term">Term as char array to be checked for token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term is stemmed word</returns>
        private bool IsStemmedWord(char[] term, int startIndex, int endIndex)
        {
            var nextNode = GetStemmedWordLastNode(term, startIndex, endIndex);
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Gets stemmed word term last node
        /// </summary>
        private NodeWithValue<string> GetStemmedWordLastNode(string term)
        {
            return GetStemmedWordLastNode(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Gets stemmed word term last node
        /// </summary>
        private NodeWithValue<string> GetStemmedWordLastNode(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _stemmedWordDictionary;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return null;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode;
        }

        private readonly NodeWithValue<string> _stemmedTokenDictionary;

        /// <summary>
        /// Adds stemmed term to dictionary with stemmed text
        /// </summary>
        /// <param name="term">term that is stemmed.</param>
        /// <param name="stemmed">Stemmed text for token</param>
        public void AddStemmedToken(string term, string stemmed)
        {
            var currentNode = _stemmedTokenDictionary;
            term = term.ToLowerInvariant();

            foreach (char termChar in term)
            {
                var charNode = currentNode.GetNode(termChar);
                if (charNode == null)
                {
                    charNode = currentNode.AddNode(termChar);
                }
                currentNode = charNode;
            }
            currentNode.IsEnd = true;
            currentNode.Value = stemmed;
        }

        /// <summary>
        /// Return if term is stemmed token
        /// </summary>
        /// <param name="term">Term to be checked in dictionary</param>
        /// <returns>true if the specified term is stemmed token</returns>
        public bool IsStemmedToken(string term)
        {
            return IsStemmedToken(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Return if term is stemmed token
        /// </summary>
        /// <param name="term">Term as char array to be checked for token</param>
        /// <param name="startIndex">start index to be searched in dictionary</param>
        /// <param name="endIndex">end index to be searched in dictionary inclusive</param>
        /// <returns>true if the specified term is stemmed token</returns>
        private bool IsStemmedToken(char[] term, int startIndex, int endIndex)
        {
            var nextNode = GetStemmedTokenLastNode(term, startIndex, endIndex);
            return nextNode != null && nextNode.IsEnd;
        }

        /// <summary>
        /// Gets stemmed token term last node
        /// </summary>
        private NodeWithValue<string> GetStemmedTokenLastNode(string term)
        {
            return GetStemmedTokenLastNode(term.ToLowerInvariant().ToCharArray(), 0, term.Length - 1);
        }

        /// <summary>
        /// Gets stemmed token term last node
        /// </summary>
        private NodeWithValue<string> GetStemmedTokenLastNode(char[] term, int startIndex, int endIndex)
        {
            var nextNode = _stemmedTokenDictionary;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (nextNode == null)
                {
                    return null;
                }
                nextNode = nextNode.GetNode(term[i]);
            }
            return nextNode;
        }

        /// <summary>
        /// Return stemmed text for the term
        /// </summary>
        /// <param name="term">Misspelled term for stemmed term</param>
        /// <param name="isToken">Out parameter detemining if stemmed is token or word</param>
        /// <returns>Stemmed if exists else null</returns>
        public string GetStemmedForText(string term, out bool? isToken)
        {
            var stemmedWord = GetStemmedWordLastNode(term);
            if (stemmedWord != null && stemmedWord.IsEnd)
            {
                isToken = false;
                return stemmedWord.Value;
            }

            var stemmedToken = GetStemmedTokenLastNode(term);
            if (stemmedToken != null && stemmedToken.IsEnd)
            {
                isToken = true;
                return stemmedToken.Value;
            }

            isToken = null;
            return null;
        }

        #endregion

        #region Identified

        private readonly List<string> _identifiedList;

        /// <summary>
        /// Adds dictionary words found in system to token dictionary
        /// </summary>
        /// <param name="word">Dictionary word found in system</param>
        public void AddIdentifiedInProject(string word)
        {
            if (!_identifiedList.Contains(word))
            {
                _identifiedList.Add(word);
            }
        }

        /// <summary>
        /// Gets list of all identified words
        /// </summary>
        /// <returns>List of all identified word</returns>
        public IEnumerable<string> GetProjectIdentifiedsAndTokens()
        {
            return _identifiedList;
        }

        #endregion
    }
}
