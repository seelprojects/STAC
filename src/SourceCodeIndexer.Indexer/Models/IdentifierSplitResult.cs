using System.Collections.Generic;
using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.Models
{
    /// <summary>
    /// Holds individual split result for each identifier
    /// </summary>
    public class IdentifierSplitResult
    {
        public IdentifierSplitResult(string identifier, IndexerFile indexerFile)
        {
            Identifier = identifier;
            IndexerFile = indexerFile;

            _splits = new List<SplitWithIdentification>();
        }

        /// <summary>
        /// Indexer File this split belongs to
        /// </summary>
        public IndexerFile IndexerFile { get; set; }

        /// <summary>
        /// Identifier that split result is based on
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Split result of identifier
        /// </summary>
        private readonly List<SplitWithIdentification> _splits;

        /// <summary>
        /// Splits for identifier
        /// </summary>
        public IReadOnlyList<SplitWithIdentification> Splits => _splits.AsReadOnly();

        /// <summary>
        /// Adds item to split list
        /// </summary>
        /// <param name="split">String which is part of splitted identifier</param>
        /// <param name="identification">Identification type of split as <see cref="SplitIdentification"/></param>
        public void Add(string split, SplitIdentification identification)
        {
            _splits.Add(new SplitWithIdentification(split, identification));
        }

        /// <summary>
        /// Adds item to split list
        /// </summary>
        /// <param name="splitWithIdentification">Identification result to be added</param>
        public void Add(SplitWithIdentification splitWithIdentification)
        {
            _splits.Add(splitWithIdentification);
        }

        /// <summary>
        /// Adds item to split list
        /// </summary>
        /// <param name="splitWithIdentifications"></param>
        public void Add(List<SplitWithIdentification> splitWithIdentifications)
        {
            _splits.AddRange(splitWithIdentifications);
        }
    }
}
