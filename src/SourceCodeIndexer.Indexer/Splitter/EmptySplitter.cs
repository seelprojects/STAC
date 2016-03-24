using System.Collections.Generic;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.Splitter
{
    public class EmptySplitter : SplitterBase
    {
        public override string GetDisplayame()
        {
            return "No Split";
        }

        protected override List<SplitWithIdentification> ApplySplit(string identifier)
        {
            return new List<SplitWithIdentification>()
            {
                new SplitWithIdentification(identifier.ToLowerInvariant(),
                    GetSplitIdentification(identifier.ToLowerInvariant()))
            };
        }
    }
}
