using SourceCodeIndexer.STAC.Dictionaries;

namespace SourceCodeIndexer.STAC.TextCorrector
{
    public interface ITextCorrector
    {
        /// <summary>
        /// Sets dictionary to be used
        /// </summary>
        /// <param name="dictionary">Dictionary to be used. Requiered if use dictionary is set to true.</param>
        void SetDictionary(IDictionary dictionary);

        /// <summary>
        /// Enables/Disables use of dictionary.
        /// </summary>
        /// <param name="useDictionary">True if dictionary is to be used. Must set dictionary if true.</param>
        void UseDictionary(bool useDictionary);

        /// <summary>
        /// Sets token dictionary to be used
        /// </summary>
        /// <param name="tokenDictionary">Token dictionary to be used. Requiered if use token dictionary is set to true.</param>
        void SetTokenDictionary(ITokenDictionary tokenDictionary);

        /// <summary>
        /// Enables/Disables use of token dictionary.
        /// </summary>
        /// <param name="useTokenDictionary">True if token dictionary is to be used. Must set token dictionary if true.</param>
        void UseTokenDictionary(bool useTokenDictionary);

        /// <summary>
        /// Gets corrected form of the text
        /// </summary>
        /// <param name="text">Text to be corrected</param>
        /// <returns>Corrected text. Null is returned if no match is found.</returns>
        string Correct(string text);
    }
}
