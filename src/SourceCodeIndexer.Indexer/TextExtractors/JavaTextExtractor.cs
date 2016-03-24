﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SourceCodeIndexer.STAC.TextExtractors
{
    public class JavaTextExtractor : TextExtractorBase
    {
        public JavaTextExtractor()
        {
            CommentsAndStringLiteralsRegexStringRegex = new Regex(RegularExpressions.JavaRegexCommentAndStringLiteral, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            AnnotationRegexStringRegex = new Regex(RegularExpressions.JavaRegexAnnotations, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            GenericTypeRegexStringRegex = new Regex(RegularExpressions.JavaRegexGenericType, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            ReturnKeywordRegexStringRegex = new Regex(RegularExpressions.JavaRegexReturnKeyword, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            KeywordsRegexStringRegex = new Regex(RegularExpressions.JavaRegexKeywords, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            MethodDeclarationRegexStringRegex = new Regex(RegularExpressions.JavaRegexMethodDeclaration, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            VariableDeclarationRegexStringRegex = new Regex(RegularExpressions.JavaRegexVariableDeclaration, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
            PackageDeclarationAndImportStringRegex = new Regex(RegularExpressions.JavaRegexPackageDeclarationAndImport, RegexOptions.None, TimeSpan.FromSeconds(IndexerResources.SplitTimeout));
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
            return new List<string>() {".java"};
        }
    }
}