using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC.FileStats
{
    public class CPlusPlusFileStatReader : FileStatReaderBase
    {
        public CPlusPlusFileStatReader()
        {
            CommentsAndStringLiteralsRegexStringRegex = new Regex(RegularExpressions.CSharpRegexCommentAndStringLiteral);
        }

        public override Regex CommentsAndStringLiteralsRegexStringRegex { get; }
    }
}
