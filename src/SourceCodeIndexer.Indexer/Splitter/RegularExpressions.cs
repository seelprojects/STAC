namespace SourceCodeIndexer.STAC.Splitter
{
    static class RegularExpressions
    {
        #region CamelCase Splitter

        public const string RegexCamelcaseSplitterOnSymbols = "[^a-zA-Z]+";
        public const string RegexCamelcaseSplitterOnTransition = "(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z](?:s[a-z]|[abcdefghijklmnopqrtuvwxyz]))";
        public const string RegexAllCaps = "^[A-Z]+$";
        public const string RegexLeadingAndTrailingNonAlphabet = "^[^A-Za-z]+|[^A-Za-z]+$";

        public const string RegexNonWords = "[^A-Za-z]";
        public const string StringEmpty = "";
        public const string Space = " ";

        public const string EscapedCharacters = @"\\n|\\r|\\t|\\\\n|\\\\r|\\\\t";

        #endregion
    }
}