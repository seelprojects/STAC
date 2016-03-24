using SourceCodeIndexer.STAC.FileStats;

namespace SourceCodeIndexer.UI.ViewModels
{
    public class LocMetricWindowViewModel : ViewModelBase
    {
        #region Properties

        public string Title { get; set; }

        public int SourceFile { get; set; }

        public int Lines { get; set; }

        public int LinesOfCode { get; set; }

        public int LinesOfComments { get; set; }

        public int LinesOfCodeAndComments { get; set; }

        public int BlankLines { get; set; }

        #endregion

        public void LoadStats(ProjectStat projectStat)
        {
            if (projectStat == null)
                return;

            SourceFile = projectStat.TotalFilesCount;
            Lines = projectStat.TotalLines;
            LinesOfCode = projectStat.TotalLinesOfCode;
            LinesOfComments = projectStat.TotalLinesOfComment;
            LinesOfCodeAndComments = projectStat.TotalLinesOfCodeAndComment;
            BlankLines = projectStat.EmptyLines;

            NotifyPropertyChanged(() => Title);

            NotifyPropertyChanged(() => SourceFile);
            NotifyPropertyChanged(() => Lines);
            NotifyPropertyChanged(() => LinesOfCode);
            NotifyPropertyChanged(() => LinesOfComments);
            NotifyPropertyChanged(() => LinesOfCodeAndComments);
            NotifyPropertyChanged(() => BlankLines);
        }
    }
}
