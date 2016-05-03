using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC.FileStats
{
    public class CSharpFileStatReader : FileStatReaderBase
    {
        public CSharpFileStatReader()
        {
            CommentsAndStringLiteralsRegexStringRegex = new Regex(RegularExpressions.CSharpRegexCommentAndStringLiteral);
        }

        public override Regex CommentsAndStringLiteralsRegexStringRegex { get; }
    }
}
