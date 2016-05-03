using System;

namespace SourceCodeIndexer.STAC.Enum
{
    public class NameAttribute : Attribute
    {
        private readonly string _name;

        public NameAttribute(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
