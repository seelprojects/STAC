namespace SourceCodeIndexer.STAC.Models
{
    /// <summary>
    /// Holds file path and name for easy access
    /// </summary>
    public class IndexerFile
    {
        public IndexerFile(string path, string name, string extension)
        {
            Path = path;
            Name = name;
            Extension = extension.ToLowerInvariant();
        }

        /// <summary>
        /// Actual path of the file
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// File name for display purpose
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Extension of file
        /// </summary>
        public string Extension { get; }
    }
}
