using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC.FileStats
{
    public class JavaFileStatReader : FileStatReaderBase
    {
        public JavaFileStatReader()
        {
            CommentsAndStringLiteralsRegexStringRegex = new Regex(RegularExpressions.JavaRegexCommentAndStringLiteral);
        }

        public override Regex CommentsAndStringLiteralsRegexStringRegex { get; }
    }
}
