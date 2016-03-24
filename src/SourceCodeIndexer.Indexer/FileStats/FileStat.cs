using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.FileStats
{
    public class FileStat
    {
        public IndexerFile IndexerFile { get; set; }

        public int TotalLines { get; set; }

        public int TotalLinesOfCode { get; set; }

        public int TotalLinesOfComment { get; set; }

        public int TotalLinesOfCodeAndComment { get; set; }

        public int EmptyLines { get; set; }

        /// <summary>
        /// Adds another file stat to this one
        /// </summary>
        /// <param name="fileStat">File Stat to be added</param>
        public void Add(FileStat fileStat)
        {
            TotalLines += fileStat.TotalLines;
            TotalLinesOfCode += fileStat.TotalLinesOfCode;
            TotalLinesOfComment += fileStat.TotalLinesOfComment;
            TotalLinesOfCodeAndComment += fileStat.TotalLinesOfCodeAndComment;
            EmptyLines += fileStat.EmptyLines;
        }
    }
}
