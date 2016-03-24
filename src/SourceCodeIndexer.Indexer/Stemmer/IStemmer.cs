namespace SourceCodeIndexer.STAC.Stemmer
{
    public interface IStemmer
    {
        /// <summary>
        /// Gets stem text
        /// </summary>
        /// <param name="text">Text to stem</param>
        /// <returns>Text that is stemmed</returns>
        string GetStemmedText(string text);
    }
}
