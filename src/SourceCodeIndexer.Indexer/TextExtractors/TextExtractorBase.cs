using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.TextExtractors
{
    public abstract class TextExtractorBase : ITextExtractor
    {
        protected TextExtractorBase()
        {
            WordMatchStringRegex = new Regex(RegularExpressions.WordRegex);
            SplitOnWhiteSpaceRegex = new Regex(RegularExpressions.RegexCamelcaseSplitterOnSymbols);
            WhiteSpaceRegex = new Regex(RegularExpressions.RegexWhiteSpace);
        }

        #region Regex 

        private Regex WhiteSpaceRegex { get; }

        /// <summary>
        /// determines regular expression for comment and string literals
        /// </summary>
        public abstract Regex CommentsAndStringLiteralsRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals
        /// </summary>
        public abstract Regex AnnotationRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for generic types from text with no comments and string literals
        /// </summary>
        public abstract Regex GenericTypeRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for java keywords from text with no comments and string literals
        /// </summary>
        public abstract Regex ReturnKeywordRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for java keywords from text with no comments and string literals
        /// </summary>
        public abstract Regex KeywordsRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for package declaration and imports
        /// </summary>
        public abstract Regex PackageDeclarationAndImportStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals, annotations and java keywords
        /// </summary>
        public abstract Regex MethodDeclarationRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals, annotations and java keywords
        /// </summary>
        public abstract Regex VariableDeclarationRegexStringRegex { get; }

        /// <summary>
        /// determines regular expression for annotations from text with no comments and string literals, annotations and java keywords
        /// </summary>
        public Regex WordMatchStringRegex { get; }

        /// <summary>
        /// Regex to split in white space
        /// </summary>
        public Regex SplitOnWhiteSpaceRegex { get; }

        #endregion

        /// <summary>
        /// Determines what file extension is used for this extractor
        /// </summary>
        public abstract IList<string> FileExtensionFor();

        /// <summary>
        /// Extract everything from source code. This is generally used to create token dictionary
        /// </summary>
        /// <param name="fileText">Source code in which the extraction is done</param>
        /// <returns>List of strings</returns>
        public IEnumerable<string> Extract(string fileText)
        {
            return ExtractAll(fileText);
        }

        /// <summary>
        /// Extract comments and string literals and/or identifiers from source code
        /// </summary>
        /// <param name="text">Source code in which the extraction is done</param>
        /// <param name="extractType">Identifies split only comments/String literals, only identifiers or both of type <see cref="ExtractType"/></param>
        /// <returns>List of strings</returns>
        public IEnumerable<string> Extract(string text, ExtractType extractType)
        {
            if ((extractType & ExtractType.Comments) == ExtractType.Comments)
            {
                foreach (string item in ExtractComments(text))
                {
                    yield return item;
                }
            }

            if ((extractType & ExtractType.IdentifiersAndStringLiterals) == ExtractType.IdentifiersAndStringLiterals)
            {
                foreach (string item in ExtractIdentifiersAndStringLiterals(text))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Extracts comments 
        /// </summary>
        /// <param name="text">Text to perform extract on</param>
        /// <returns>List of comments</returns>
        public IEnumerable<string> ExtractComments(string text)
        {
            MatchCollection matches = CommentsAndStringLiteralsRegexStringRegex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Groups[RegularExpressions.GroupInlineCommentName].Value != "")
                {
                    foreach (string commentWord in SplitOnWhiteSpaceRegex.Split(match.Groups[RegularExpressions.GroupInlineCommentName].Value))
                    {
                        if (commentWord != "")
                            yield return commentWord;
                    }
                }

                if (match.Groups[RegularExpressions.GroupMultilineCommentName].Value != "")
                {
                    foreach (string commentWord in SplitOnWhiteSpaceRegex.Split(match.Groups[RegularExpressions.GroupMultilineCommentName].Value))
                    {
                        if (commentWord != "")
                            yield return commentWord;
                    }
                }
            }
        }

        /// <summary>
        /// Extracts string literals 
        /// </summary>
        /// <param name="text">Text to perform extract on</param>
        /// <returns>List of string literals</returns>
        public IEnumerable<string> ExtractStringLiterals(string text)
        {
            MatchCollection matches = CommentsAndStringLiteralsRegexStringRegex.Matches(text);
            foreach (string stringLiterals in (from Match match in matches
                where match.Groups[RegularExpressions.GroupStringLiteralsName].Value != ""
                select match.Groups[RegularExpressions.GroupStringLiteralsName].Value))
            {
                foreach (string stringLiteralWord in SplitOnWhiteSpaceRegex.Split(stringLiterals))
                {
                    if (stringLiteralWord != "")
                        yield return stringLiteralWord;
                }
            }
        }

        /// <summary>
        /// Extracts everyting in source code
        /// </summary>
        /// <param name="text">Text to extract from</param>
        /// <returns>List of identifiers</returns>
        public IEnumerable<string> ExtractAll(string text)
        {
            MatchCollection wordMatches = WordMatchStringRegex.Matches(text);
            return (from Match match in wordMatches
                    where match.Groups[0].Value != ""
                    select match.Groups[0].Value);
        }

        /// <summary>
        /// Extracts everyting: String literals, method names and identifier names
        /// </summary>
        /// <param name="text">Text to extract from</param>
        /// <returns>List of identifiers</returns>
        public IEnumerable<string> ExtractIdentifiersAndStringLiterals(string text)
        {
            // Step 1 : Remove package declaration
            text = RemovePackageDeclarationAndImports(text);

            // Step 2: remove keywords
            text = RemoveKeywords(text);

            // Step 3: Extract String Literals
            foreach (string item in ExtractStringLiterals(text))
            {
                yield return item;
            }

            // Step 4: Remove comments and string literals
            text = RemoveCommentsAndStringLiterals(text);

            // Step 5: now everything is plain source code. remove annotations
            text = RemoveAnnotations(text);

            // Step 6: remove generic type parameter
            text = RemoveGenericTypeParameters(text);

            // Step 7: Replace all white space by single
            text = WhiteSpaceRegex.Replace(text, RegularExpressions.Space);

            MatchCollection methodMatches = MethodDeclarationRegexStringRegex.Matches(text);
            IEnumerable<string> methodEnumerable = (from Match match in methodMatches
                                                    where match.Groups[1].Value != ""
                                                    select match.Groups[1].Value);
            foreach (string item in methodEnumerable)
            {
                yield return item;
            }

            MatchCollection variableMatches = VariableDeclarationRegexStringRegex.Matches(text);
            foreach (Match match in variableMatches)
            {
                int group = 1;
                while (match.Groups[group].Value != "")
                {
                    IEnumerable<string> variableEnumerable = (from Capture capture in match.Groups[@group].Captures select capture.Value);
                    foreach (string item in variableEnumerable)
                    {
                        yield return item;
                    }
                    group++;
                }
            }
        }

        /// <summary>
        /// Removes comments and string literals
        /// </summary>
        /// <param name="text">Text to remove comments and string literals</param>
        /// <remarks>It is necessary to replace them with space. Eg: static/*hello*/void is acceptable.</remarks>
        /// <returns>Text without any comments and string literals</returns>
        public string RemoveCommentsAndStringLiterals(string text)
        {
            text = CommentsAndStringLiteralsRegexStringRegex.Replace(text, RegularExpressions.Space);
            return text;
        }

        /// <summary>
        /// Removes annotation from text with no comments and string literals
        /// </summary>
        /// <param name="literalsAndCommentsRemovedtext">text from which string literals and comments are removed</param>
        /// <returns>text with no annotations</returns>
        public string RemoveAnnotations(string literalsAndCommentsRemovedtext)
        {
            string text = AnnotationRegexStringRegex.Replace(literalsAndCommentsRemovedtext, RegularExpressions.Space);
            return text;
        }

        /// <summary>
        /// Removes java return keywords so that it wont be confused with data type
        /// </summary>
        /// <param name="literalsAndCommentsRemovedtext">text from which string literals and comments are removed</param>
        /// <returns>text with no return</returns>
        public string RemoveReturnKeywords(string literalsAndCommentsRemovedtext)
        {
            string text = ReturnKeywordRegexStringRegex.Replace(literalsAndCommentsRemovedtext, RegularExpressions.StringEmpty);
            return text;
        }

        /// <summary>
        /// Removes java specific keywords
        /// </summary>
        /// <param name="literalsAndCommentsRemovedtext">text from which string literals and comments are removed</param>
        /// <returns>text with no annotations</returns>
        public string RemoveKeywords(string literalsAndCommentsRemovedtext)
        {
            string text = KeywordsRegexStringRegex.Replace(literalsAndCommentsRemovedtext, RegularExpressions.Space);
            return text;
        }

        /// <summary>
        /// Removes Package declaration and imports
        /// </summary>
        /// <param name="literalsAndCommentsRemovedtext">Text to remove package declaration and imports</param>
        /// <returns>Text without any package declaration and imports</returns>
        public string RemovePackageDeclarationAndImports(string literalsAndCommentsRemovedtext)
        {
            string text = PackageDeclarationAndImportStringRegex.Replace(literalsAndCommentsRemovedtext, RegularExpressions.Space);
            return text;
        }

        /// <summary>
        /// Removes generic type parameters
        /// </summary>
        /// <param name="literalsAndCommentsRemovedtext">text from which string literals and comments are removed</param>
        /// <returns>text with no annotations</returns>
        public string RemoveGenericTypeParameters(string literalsAndCommentsRemovedtext)
        {
            while (GenericTypeRegexStringRegex.IsMatch(literalsAndCommentsRemovedtext))
            {
                literalsAndCommentsRemovedtext = GenericTypeRegexStringRegex.Replace(literalsAndCommentsRemovedtext, RegularExpressions.StringEmpty);
            }
            return literalsAndCommentsRemovedtext;
        }
    }
}