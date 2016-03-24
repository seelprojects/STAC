namespace SourceCodeIndexer.STAC.Notification
{
    public enum NotificationType
    {
        AnalyzingFile,
        IdentifyingToken,
        ReadingFileForIdentifiers,
        Splitting,
        Stemming,
        IndexingCompleted,
    }

    public enum QuestionType
    {
        NoTextExtratorDefined,
        ErrorReadingFile,
    }
}
