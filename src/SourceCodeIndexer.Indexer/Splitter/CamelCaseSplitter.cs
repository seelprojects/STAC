using System.Collections.Generic;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Splitter
{
    public class CamelCaseSplitter : SplitterBase
    {
        public CamelCaseSplitter(bool isPrimarySplitter)
        {
            IsPrimarySplitter = isPrimarySplitter;
        }

        /// <summary>
        /// Returns Display name of splitter
        /// </summary>
        /// <returns>Display Name of splitter</returns>
        public override string GetDisplayame()
        {
            return "Camel-Case";
        }

        /// <summary>
        /// Gets initial Splitter for for Camel-Case splitter
        /// </summary>
        /// <returns>Initial Splitter of type <see cref="SplitterBase"/> for Camel-Case splitter</returns>
        /// <remarks>Camel-Case splitter does not use any initial Splitter</remarks>
        protected override SplitterBase InitialSplitter => null;

        /// <summary>
        /// Primary Split implemented by Camel-Case splitter
        /// </summary>
        /// <param name="identifier">Identifier to split</param>
        /// <returns>Identifier Split Result</returns>
        protected override List<SplitWithIdentification> ApplySplit(string identifier)
        {
            return CamelCaseSplitIdentifications(identifier);
        }
    }
}
