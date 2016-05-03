using System.Collections.Generic;
using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.TextExtractors
{
    public interface ITextExtractor
    {
        /// <summary>
        /// Extract everything from source code. This is generally used to create token dictionary
        /// </summary>
        /// <param name="fileText">Source code in which the extraction is done</param>
        /// <returns>List of strings</returns>
        IEnumerable<string> Extract(string fileText);

        /// <summary>
        /// Extract comments and string literals and/or identifiers from source code
        /// </summary>
        /// <param name="fileText">Source code in which the extraction is done</param>
        /// <param name="extractType">Identifies split only comments/String literals, only identifiers or both of type <see cref="ExtractType"/></param>
        /// <returns>List of strings</returns>
        IEnumerable<string> Extract(string fileText, ExtractType extractType);

        /// <summary>
        /// Determines what file extension is used for this extractor
        /// </summary>
        IList<string> FileExtensionFor();
    }
}
