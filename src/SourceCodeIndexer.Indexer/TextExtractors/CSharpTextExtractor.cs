using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SourceCodeIndexer.STAC.TextExtractors
{
    public class CSharpTextExtractor : TextExtractorBase
    {
        public CSharpTextExtractor()
        {
            CommentsAndStringLiteralsRegexStringRegex = new Regex(RegularExpressions.CSharpRegexCommentAndStringLiteral, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            AnnotationRegexStringRegex = new Regex(RegularExpressions.CSharpRegexAnnotations, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            GenericTypeRegexStringRegex = new Regex(RegularExpressions.CSharpRegexGenericType, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            ReturnKeywordRegexStringRegex = new Regex(RegularExpressions.CSharpRegexReturnKeyword, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            KeywordsRegexStringRegex = new Regex(RegularExpressions.CSharpRegexKeywords, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            MethodDeclarationRegexStringRegex = new Regex(RegularExpressions.CSharpRegexMethodDeclaration, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            VariableDeclarationRegexStringRegex = new Regex(RegularExpressions.CSharpRegexVariableDeclaration, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            PackageDeclarationAndImportStringRegex = new Regex(RegularExpressions.CSharpRegexPackageDeclarationAndImport, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
        }

        /// <summary>
        /// determines regular expression for comment and string literals
        /// </summary>
        public override Regex CommentsAndStringLiteralsRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals
        /// </summary>
        public override Regex AnnotationRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for generic types from text with no comments and string literals
        /// </summary>
        public override Regex GenericTypeRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for java keywords from text with no comments and string literals
        /// </summary>
        public override Regex ReturnKeywordRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for java keywords from text with no comments and string literals
        /// </summary>
        public override Regex KeywordsRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for package declaration and imports
        /// </summary>
        public override Regex PackageDeclarationAndImportStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals, annotations and java keywords
        /// </summary>
        public override Regex MethodDeclarationRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals, annotations and java keywords
        /// </summary>
        public override Regex VariableDeclarationRegexStringRegex { get; }

        /// <summary>
        /// Determines what file extension is used for this extractor
        /// </summary>
        public override IList<string> FileExtensionFor()
        {
            return new List<string>() { ".cs" };
        }
    }
}