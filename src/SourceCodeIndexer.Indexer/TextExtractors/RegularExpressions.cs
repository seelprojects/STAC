namespace SourceCodeIndexer.STAC.TextExtractors
{
    public static class RegularExpressions
    {
        public const string GroupInlineCommentName = "InlCmt";
        public const string GroupMultilineCommentName = "MltCmt";
        public const string GroupStringLiteralsName = "StrLit";

        #region Java

        // for string literals we wont match quotation mark so that the quotation mark remains in the source code to identify variables
        // for comments we match \\ or \* *\ so that they are removed from source code
        public const string JavaRegexCommentAndStringLiteral = "(?:(?<!['\\\\])(?:\")(?<" + GroupStringLiteralsName + ">.*?)(?!\\\\)(?:\"))|(?<" + GroupInlineCommentName + ">//.*)|(?<" + GroupMultilineCommentName + ">(?s)/\\*.*?\\*/)";
        public const string JavaRegexAnnotations = @"@[\w]+(?:\s*\([^\)]*\))?";
        public const string JavaRegexGenericType = @"<[^;\{\}<>]*>";
        public const string JavaRegexVariableDeclaration = @"[\w\$\[\]\.]+\s+([\w\$]+)\s*(?:(?:=\s*[^;,\}\)]+\s*)?(?:,\s*(?:[\w\$\[\]\.]+\s+)?\s*([\w\$]+)\s*(?:=\s*[^;,\}\)]+\s*)?)*(?:;|\)\s*\{|\}|\)))";
        public const string JavaRegexMethodDeclaration = @"[\$\w\[\]\.]+\s+([\$\w]+)\s*\([^\)]*\)?\s*(?:throws\s+[^{]+)?[\{]";
        public const string JavaRegexPackageDeclarationAndImport = @"(package|import)\s+[\w\.]+;";
        /// <summary>
        /// do not remove 'throws' used by method declarations, data type including void for variable/method delcaration identification, null are value so don't remove them too
        /// </summary>
        public const string JavaRegexKeywords = @"(?<!\w)(abstract|continue|for|new|switch|assert|default|goto|package|synchronized|do|if|private|this|break|implements|protected|" +
            "else|import|public|case|enum|instanceof|return|transient|catch|extends|try|final|interface|static|class|finally|strictfp|volatile|const|" +
            @"native|super|while)(?!\w)";
        public const string JavaRegexReturnKeyword = "return ";

        #endregion

        #region C#

        public const string CSharpRegexCommentAndStringLiteral = "(?:(?<!['\\\\])(?:@?\")(?<" + GroupStringLiteralsName + ">(?s).*?)(?!\\\\)(?:\"))|(?<" + GroupInlineCommentName + ">//.*)|(?<" + GroupMultilineCommentName + ">(?s)/\\*.*?\\*/)";
        public const string CSharpRegexGenericType = @"<[^;\{\}<>]*>";
        public const string CSharpRegexAnnotations = @"[ \t]*#\w+.*";
        public const string CSharpRegexVariableDeclaration = @"[\w\$\[\]\.]+\s+([\w\$]+)\s*(?:(?:=\s*[^;,\}\)]+\s*)?(?:,\s*(?:[\w\$\[\]\.]+\s+)?\s*([\w\$]+)\s*(?:=\s*[^;,\}\)]+\s*)?)*(?:;|\{|\)\s*\{|\}|\)))";
        public const string CSharpRegexMethodDeclaration = @"[\$\w\[\]\.]+\s+([\$\w]+)\s*\([^\)]*\)?\s*\{";
        public const string CSharpRegexPackageDeclarationAndImport = @"using\s+.+;";
        /// <summary>
        /// do not remove 'throws' used by method declarations, data type including void for variable/method delcaration identification, null are value so don't remove them too
        /// </summary>
        public const string CSharpRegexKeywords = @"(?<!\w)(abstract|as|base|break|case|catch|checked|class|const|continue|default|delegate|do|else|enum|event|explicit|" +
            "extern|finally|fixed|for|foreach|goto|if|implicit|in|internal|is|lock|namespace|new|operator|out|override|params|private|protected|public|readonly|ref|" +
            @"return|sealed|sizeof|stackalloc|static|struct|switch|this|throw|try|typeof|unchecked|unsafe|using|virtual|volatile|while)(?!\w)";
        public const string CSharpRegexReturnKeyword = "return ";

        #endregion

        #region C++

        public const string CPlusPlusRegexCommentAndStringLiteral = "(?:(?<!['\\\\])(?:\")(?<" + GroupStringLiteralsName + ">.*?)(?!\\\\)(?:\"))|(?<" + GroupInlineCommentName + ">//.*)|(?<" + GroupMultilineCommentName + ">(?s)/\\*.*?\\*/)";
        public const string CPlusPlusRegexGenericType = @"(<[^;\{\}<>]*>)|(\w+::)";
        public const string CPlusPlusRegexAnnotations = "@";
        //public const string CPlusPlusRegexVariableDeclaration = @"[\w\$\[\]\.]+(?:\s*(?:\**|&*)\s*)\s+([\w\$]+)\s*(?:=\s*[^;,\}\)]+\s*)?(?:,\s*(?:[\w\$\[\]\.]+)?(?:\**\s+|\s+\**|\s+\**\s+|\s+&?)([\w\$]+)\s*(?:=\s*[^;\s]+\s*)?)*(?:;|:|\)\s*(?:\{|:))";
        public const string CPlusPlusRegexVariableDeclaration = @"[\w\$\[\]\.]+(?:\s*(?:\**|&*)\s*)\s+([\w\$]+)\s*(?:(?:=\s*[^;,\}\)\(]+\s*(?:\(.*\))?)?(?:,\s*(?:[\w\$\[\]\.]+\s+)?(?:\s*(?:\**|&*)\s*)([\w\$]+)\s*(?:=\s*[^;,\}\)]+\s*)?)*(?:;|\{\s*(?::|\{)|\}|\)))";
        public const string CPlusPlusRegexMethodDeclaration = @"[\$\w\[\]\.]+\s+([\$\w]+)\s*\([^\)]*\)?\s*\{";
        public const string CPlusPlusRegexPackageDeclarationAndImport = "(#include[ \\t]+[<>\\w\\.\"\"\\\\/]*)|(using\\s+(namespace\\s+)?(\\w+::)*\\w+;)";
        /// <summary>
        /// do not remove 'throws' used by method declarations, data type including void for variable/method delcaration identification, null are value so don't remove them too
        /// </summary>
        public const string CPlusPlusRegexKeywords = @"(?<!\w)(alignas|alignof|and|and_eq|asm|bitand|bitor|break|case|catch|class|compl|concept|const|constexpr|const_cast|continue|" + 
            "default|delete|do|dynamic_cast|else|enum|explicit|export|extern|for|friend|goto|if|inline|mutable|namespace|new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|" + 
            "protected|public|register|reinterpret_cast|requires|return|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|try|typedef|typeid|" + 
            @"typename|union|unsigned|using|virtual|voi|volatile|while|xor|xor_eq)(?!\w)";
        public const string CPlusPlusRegexReturnKeyword = "return ";

        #endregion

        public const string StringEmpty = "";
        public const string Space = " ";
        public const string WordRegex = "[A-Za-z]+";
        public const string AnythingButWhiteSpace = @"[^\s]+";
        public const string RegexCamelcaseSplitterOnSymbols = "[^a-zA-Z]+";
        public const string RegexWhiteSpace = @"\s+";
    }
}
