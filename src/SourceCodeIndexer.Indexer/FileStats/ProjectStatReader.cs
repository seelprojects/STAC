using System.Collections.Generic;
using System.IO;
using System.Linq;
using SourceCodeIndexer.STAC.Models;

namespace SourceCodeIndexer.STAC.FileStats
{
    public class ProjectStatReader
    {
        private readonly string _projectPath;
        private readonly List<string> _fileExtensionsToSearch;

        public ProjectStatReader(string projectPath, List<string> fileExtensionsToSearch)
        {
            _projectPath = projectPath;
            _fileExtensionsToSearch = fileExtensionsToSearch;
        }

        /// <summary>
        /// Return file to be indexed and also maintains the file count info
        /// </summary>
        /// <returns>List of files to index with count</returns>
        public ProjectStat GetProjectStat()
        {
            if (!Directory.Exists(_projectPath))
                return null;

            DirectoryInfo directoryInfo = new DirectoryInfo(_projectPath);
            ProjectStat projectStat = new ProjectStat()
            {
                Name = directoryInfo.Name
            };

            projectStat.FileStats.AddRange(GetAllFiles(directoryInfo));
            return projectStat;
        }

        /// <summary>
        /// Returns a list of all java files in the give directory and its sub-dirs
        /// </summary>
        /// <param name="directoryInfo">Directory to search</param>
        /// <returns>List of java files</returns>
        public List<FileStat> GetAllFiles(DirectoryInfo directoryInfo)
        {
            List<FileStat> returnFileStats = new List<FileStat>();

            // load files in this dir
            returnFileStats.AddRange(directoryInfo.EnumerateFiles()
                .Where(file => _fileExtensionsToSearch.Contains(file.Extension.ToLowerInvariant()))
                .Select(file => new FileStat() {IndexerFile = new IndexerFile(file.FullName, file.Name, file.Extension)}));

            // recursively load file in sub dirs
            directoryInfo.GetDirectories().ToList().ForEach(x => returnFileStats.AddRange(GetAllFiles(x)));

            return returnFileStats;
        }
    }

    public class ProjectStat
    {
        public ProjectStat()
        {
            FileStats = new List<FileStat>();
        }

        public List<FileStat> FileStats { get; }

        public string Name { get; set; }

        public int TotalFilesCount => FileStats.Count;

        public int TotalLines
        {
            get
            {
                int count = 0;
                FileStats.ForEach(x => count += x.TotalLines);
                return count;
            }
        }

        public int TotalLinesOfCode
        {
            get
            {
                int count = 0;
                FileStats.ForEach(x => count += x.TotalLinesOfCode);
                return count;
            }
        }

        public int TotalLinesOfComment
        {
            get
            {
                int count = 0;
                FileStats.ForEach(x => count += x.TotalLinesOfComment);
                return count;
            }
        }

        public int TotalLinesOfCodeAndComment
        {
            get
            {
                int count = 0;
                FileStats.ForEach(x => count += x.TotalLinesOfCodeAndComment);
                return count;
            }
        }

        public int EmptyLines
        {
            get
            {
                int count = 0;
                FileStats.ForEach(x => count += x.EmptyLines);
                return count;
            }
        }
    }
}
