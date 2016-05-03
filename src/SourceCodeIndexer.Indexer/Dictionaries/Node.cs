using System.Collections.Generic;

namespace SourceCodeIndexer.STAC.Dictionaries
{
    internal class NodeWithValue<T>
    {
        public T Value { get; set; }

        /// <summary>
        /// Marks end of word from root
        /// </summary>
        public bool IsEnd { get; set; }

        private readonly Dictionary<char, NodeWithValue<T>> _children;

        internal NodeWithValue()
        {
            _children = new Dictionary<char, NodeWithValue<T>>();
        }

        /// <summary>
        /// Return node repersented by the character
        /// </summary>
        /// <param name="nodeFor">Character to search for in the children of current node</param>
        /// <returns>Node the character repersents. Null if does not exists.</returns>
        internal NodeWithValue<T> GetNode(char nodeFor)
        {
            if (NodeExists(nodeFor))
            {
                return _children[nodeFor];
            }
            return null;
        }

        /// <summary>
        /// Adds an item in dictionary with given char and empty node
        /// </summary>
        /// <param name="nodeFor">Char to add to dictionary</param>
        /// <returns>Node for newly added node</returns>
        internal NodeWithValue<T> AddNode(char nodeFor)
        {
            if (NodeExists(nodeFor))
                throw new System.Exception("A node for char: " + nodeFor + " already exists. Cannot add duplicate. Possible loss of data.");

            NodeWithValue<T> newNode = new NodeWithValue<T>();
            _children.Add(nodeFor, newNode);

            return newNode;
        }

        /// <summary>
        /// checks if node exists for given character in this node
        /// </summary>
        /// <param name="nodeFor">Character for which the node is being searched</param>
        /// <returns>True if node exists; else false</returns>
        private bool NodeExists(char nodeFor)
        {
            return _children.ContainsKey(nodeFor);
        }
    }

    internal class Node
    {
        /// <summary>
        /// Marks end of word from root
        /// </summary>
        public bool IsEnd { get; set; }

        private readonly Dictionary<char, Node> _children;

        internal Node()
        {
            _children = new Dictionary<char, Node>();
        }

        /// <summary>
        /// Return node repersented by the character
        /// </summary>
        /// <param name="nodeFor">Character to search for in the children of current node</param>
        /// <returns>Node the character repersents. Null if does not exists.</returns>
        internal Node GetNode(char nodeFor)
        {
            if (NodeExists(nodeFor))
            {
                return _children[nodeFor];
            }
            return null;
        }

        /// <summary>
        /// Adds an item in dictionary with given char and empty node
        /// </summary>
        /// <param name="nodeFor">Char to add to dictionary</param>
        /// <returns>Node for newly added node</returns>
        internal Node AddNode(char nodeFor)
        {
            if (NodeExists(nodeFor))
                throw new System.Exception("A node for char: " + nodeFor + " already exists. Cannot add duplicate. Possible loss of data.");

            Node newNode = new Node();
            _children.Add(nodeFor, newNode);

            return newNode;
        }

        /// <summary>
        /// checks if node exists for given character in this node
        /// </summary>
        /// <param name="nodeFor">Character for which the node is being searched</param>
        /// <returns>True if node exists; else false</returns>
        private bool NodeExists(char nodeFor)
        {
            return _children.ContainsKey(nodeFor);
        }
    }
}
