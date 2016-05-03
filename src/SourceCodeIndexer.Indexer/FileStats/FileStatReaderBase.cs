using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SourceCodeIndexer.STAC.TextExtractors;

namespace SourceCodeIndexer.STAC.FileStats
{
    public abstract class FileStatReaderBase
    {
        public abstract Regex CommentsAndStringLiteralsRegexStringRegex { get; }

        private readonly Regex _anyNonSpaceCharacterRegex;

        protected FileStatReaderBase()
        {
            _anyNonSpaceCharacterRegex = new Regex(RegularExpressions.AnythingButWhiteSpace);
        }

        /// <summary>
        /// Gets file stats like line nums etc
        /// </summary>
        /// <returns>Updates File Stat counts</returns>
        public void UpdateFileStatCount(FileStat fileStat)
        {
            fileStat.TotalLines = 0;
            fileStat.TotalLinesOfCode = 0;
            fileStat.TotalLinesOfComment = 0;
            fileStat.TotalLinesOfCodeAndComment = 0;
            fileStat.EmptyLines = 0;

            // 1. total lines
            string[] lines = File.ReadAllLines(fileStat.IndexerFile.Path);
            if (lines.Length == 0)
                return;

            fileStat.TotalLines = lines.Length;

            // add new line. Intentionally left last line
            for (int i = 0; i < lines.Length - 1; i++)
            {
                lines[i] = lines[i] + Environment.NewLine;
            }

            ProjectFileLine[] projectFileLines = new ProjectFileLine[lines.Length];
            projectFileLines[0] = new ProjectFileLine
            {
                StartIndex = 0,
                EndIndex = lines[0].Length - 1,
                Length = lines[0].Length
            };

            int startIndex = lines[0].Length;
            for (int counter = 1; counter < lines.Length; counter++)
            {
                var length = lines[counter].Length;
                projectFileLines[counter] = new ProjectFileLine
                {
                    StartIndex = startIndex,
                    EndIndex = startIndex + length - 1,
                    Length = length,
                };
                projectFileLines[counter - 1].NextLine = projectFileLines[counter];
                startIndex = startIndex + length;
            }

            // 2. Total lines of code comment
            // get indexes of comments. use this indexes to validate count lines of comments (there may be two comments per line or so)
            // string fileText = string.Join(Environment.NewLine, lines);
            string fileText = string.Join("", lines);
            List<Tuple<int, int>> commentsIndexes = GetCommentsIndexes(fileText);
            List<ProjectFileLine> projectFileLinesList = new List<ProjectFileLine>(projectFileLines);
            int commentIndexesCount = commentsIndexes.Count;
            for (int i = 0; i < commentIndexesCount; i++)
            {
                Tuple<int, int> commentsIndex = commentsIndexes[i];
                ProjectFileLine currentLine = projectFileLinesList.First(x => x.StartIndex <= commentsIndex.Item1 && x.EndIndex >= commentsIndex.Item1);
                int commentStartIndex = commentsIndex.Item1;
                int commentLength = commentsIndex.Item2;
                while (commentLength > 0 && currentLine != null)
                {
                    currentLine.HasComment = true;
                    //front
                    if (currentLine.StartIndex < commentStartIndex && !currentLine.HasCode)
                    {
                        currentLine.HasCode = (i == 0 || (commentsIndexes[i - 1].Item1 + commentsIndexes[i - 1].Item2 - 1) < currentLine.StartIndex)
                            ? _anyNonSpaceCharacterRegex.IsMatch(fileText.Substring(currentLine.StartIndex, commentStartIndex - currentLine.StartIndex))
                            : ((commentsIndexes[i - 1].Item1 + commentsIndexes[i - 1].Item2) < commentsIndexes[i].Item1)
                                && _anyNonSpaceCharacterRegex.IsMatch(fileText.Substring(commentsIndexes[i - 1].Item1 + commentsIndexes[i - 1].Item2, commentsIndexes[i].Item1 - (commentsIndexes[i - 1].Item1 + commentsIndexes[i - 1].Item2 - 1)));
                    }

                    //back
                    if (!currentLine.HasCode && commentStartIndex < currentLine.EndIndex && (currentLine.EndIndex - commentStartIndex + 1 - commentLength) > 0)
                    {
                        currentLine.HasCode = ((i + 1) == commentIndexesCount ||
                                               commentsIndexes[i + 1].Item1 > currentLine.EndIndex)
                            ? _anyNonSpaceCharacterRegex.IsMatch(fileText.Substring(
                                commentStartIndex + commentLength,
                                currentLine.EndIndex - commentStartIndex + 1 - commentLength))
                            : (commentsIndexes[i + 1].Item1 > (commentStartIndex + commentLength)
                               && (_anyNonSpaceCharacterRegex.IsMatch(fileText.Substring(commentStartIndex + commentLength, commentsIndexes[i + 1].Item1 - (commentStartIndex + commentLength)))));
                    }

                    // if the comment extends to next line, we can just substract the end index. If it is single line we have 
                    // tagged this line as already commented so does not matter if the comment ended before the end of this line
                    commentLength = commentLength - (currentLine.EndIndex - commentStartIndex + 1);
                    currentLine = currentLine.NextLine;
                    if (currentLine != null)
                        commentStartIndex = currentLine.StartIndex;
                }
            }

            ProjectFileLine currentFileLine = projectFileLines[0];
            while (currentFileLine != null)
            {
                if (!currentFileLine.HasCode && !currentFileLine.HasComment)
                {
                    currentFileLine.HasCode = _anyNonSpaceCharacterRegex.IsMatch(fileText.Substring(currentFileLine.StartIndex, currentFileLine.EndIndex - currentFileLine.StartIndex + 1));
                }

                if (currentFileLine.HasCode)
                    fileStat.TotalLinesOfCode += 1;
                if (currentFileLine.HasComment)
                    fileStat.TotalLinesOfComment += 1;
                if (currentFileLine.HasComment && currentFileLine.HasCode)
                    fileStat.TotalLinesOfCodeAndComment += 1;
                if (!currentFileLine.HasComment && !currentFileLine.HasCode)
                    fileStat.EmptyLines += 1;

                currentFileLine = currentFileLine.NextLine;
            }
        }

        /// <summary>
        /// Removes string literals only
        /// </summary>
        public List<Tuple<int, int>> GetCommentsIndexes(string text)
        {
            List<Tuple<int, int>> listOfTuples = new List<Tuple<int, int>>();

            MatchCollection collection = CommentsAndStringLiteralsRegexStringRegex.Matches(text);
            foreach (Match match in collection)
            {
                var capturesInline = match.Groups[RegularExpressions.GroupInlineCommentName].Captures.OfType<Capture>().OrderByDescending(capture => capture.Index);
                listOfTuples.AddRange(from capture in capturesInline where capture != null select new Tuple<int, int>(capture.Index, capture.Length));

                var capturesMultiline = match.Groups[RegularExpressions.GroupMultilineCommentName].Captures.OfType<Capture>().OrderByDescending(capture => capture.Index);
                listOfTuples.AddRange(from capture in capturesMultiline where capture != null select new Tuple<int, int>(capture.Index, capture.Length));
            }

            return listOfTuples;
        }
    }

    internal class ProjectFileLine
    {
        public bool HasComment { get; set; }

        public bool HasCode { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        // EndIndex - StartIndex + 1: Saves calculation time
        public int Length { get; set; }

        public ProjectFileLine NextLine { get; set; }
    }
}