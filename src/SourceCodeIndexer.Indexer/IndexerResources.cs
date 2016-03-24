namespace SourceCodeIndexer.STAC
{
    internal static class IndexerResources
    {
        public const int TokenCount = 2;

        public const int MinMisspelledStemmedLength = 4;
        public const int MinTokenLengthForCaps = 2;
        public const int MinTokenLength = 3; // min value of 2 below

        public const int SplitterIdentifiedScore = 3;
        public const int SplitterTokenScore = 1;
        public const int SplitterUnidentifiedScore = 0;

        public const double SplitTimeout = 3;
    }
}
