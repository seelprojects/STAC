using SourceCodeIndexer.STAC.Enum;

namespace SourceCodeIndexer.STAC.Models
{
    /// <summary>
    /// Hold string and its identification ie Identified, Unidentified or Token <see cref="SplitIdentification"/>
    /// </summary>
    public class SplitWithIdentification
    {
        public SplitWithIdentification(string split, SplitIdentification identification)
        {
            Split = split;
            SplitIdentification = identification;
        }

        /// <summary>
        /// String for which this identification is assignemd
        /// </summary>
        public string Split { get; set; }

        /// <summary>
        /// Identification type of string
        /// </summary>
        public SplitIdentification SplitIdentification { get; set; }
    }

    /// <summary>
    /// Hold split position and its identification ie Identified, Unidentified or Token <see cref="SplitIdentification"/>
    /// </summary>
    public class SplitPositionWithIdentification
    {
        public SplitPositionWithIdentification(int position, SplitIdentification identification)
        {
            Position = position;
            SplitIdentification = identification;
        }

        /// <summary>
        /// Postion for which this identification is assigned
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Identification type of string
        /// </summary>
        public SplitIdentification SplitIdentification { get; set; }
    }
}
