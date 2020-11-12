using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents a hierarchy.
    /// </summary>
    /// <typeparam name="T">Type of hierarchy node content.</typeparam>
    public class Hierarchy<T> : IEnumerable<HierarchyNode<T>>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<HierarchyNode<T>> _topNodes = new List<HierarchyNode<T>>();

        /// <summary>
        /// Top nodes of the hierarchy.
        /// </summary>
        public IReadOnlyList<HierarchyNode<T>> TopNodes => _topNodes;

        /// <summary>
        /// Checks if the hierarchy contains node with given id.
        /// </summary>
        /// <param name="id">Node id.</param>
        public bool ContainsNodeWithId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            return _topNodes.Any(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                || _topNodes.Any(n => n.ContainsSubNodeWithId(id));
        }

        /// <summary>
        /// Gets node with given id.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <returns>Return null, if there is no node with given id in the hierarchy</returns>
        public HierarchyNode<T> GetNodeById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            return this.FirstOrDefault(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds new top-level node.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <param name="content">Node content.</param>
        /// <returns>True, if the node was added. False, if given id already exists.</returns>
        public bool AddTopNode(string id, T content)
        {
            if (ContainsNodeWithId(id)) return false;

            _topNodes.Add(new HierarchyNode<T>(this, id, content));

            RenumerateNodes();

            return true;
        }

        internal void RenumerateNodes()
        {
            var serviceIndex = 0;

            foreach (var node in _topNodes)
            {
                node.Left = serviceIndex++;

                node.RenumerateSubNodes(ref serviceIndex);

                node.Right = serviceIndex++;
            }
        }

        public IEnumerator<HierarchyNode<T>> GetEnumerator()
        {
            return _topNodes.Concat(_topNodes.SelectMany(n => n)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    /// <summary>
    /// Represents one node in the hierarchy.
    /// </summary>
    /// <typeparam name="T">Type of the content.</typeparam>
    public sealed class HierarchyNode<T> : IEnumerable<HierarchyNode<T>>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<HierarchyNode<T>> _subNodes = new List<HierarchyNode<T>>();
        private readonly Hierarchy<T> _hierarchy;

        /// <summary>
        /// Subnodes of this node.
        /// </summary>
        public IReadOnlyList<HierarchyNode<T>> SubNodes => _subNodes;

        public HierarchyNode(Hierarchy<T> hierarchy, string id, T content)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            _hierarchy = hierarchy ?? throw new ArgumentNullException(nameof(hierarchy));
            Id = id;
            Content = content;
        }

        /// <summary>
        /// Id of the node.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Content of the node.
        /// </summary>
        public T Content { get; }

        /// <summary>
        /// Left service index.
        /// </summary>
        public int Left { get; internal set; }

        /// <summary>
        /// Right service index.
        /// </summary>
        public int Right { get; internal set; }

        /// <summary>
        /// Checks if this node contains subnode with given id.
        /// </summary>
        /// <param name="id">Node id.</param>
        public bool ContainsSubNodeWithId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            return _subNodes.Any(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                || _subNodes.Any(n => n.ContainsSubNodeWithId(id));
        }

        /// <summary>
        /// Adds new subnode to this node.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <param name="content">Node content.</param>
        /// <returns>True, if the node was added. False, if given id already exists in the hierarchy.</returns>
        public bool AddSubNode(string id, T content)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            if (_hierarchy.ContainsNodeWithId(id)) return false;

            _subNodes.Add(new HierarchyNode<T>(_hierarchy, id, content));

            _hierarchy.RenumerateNodes();

            return true;
        }

        internal void RenumerateSubNodes(ref int serviceIndex)
        {
            foreach (var node in _subNodes)
            {
                node.Left = serviceIndex++;

                node.RenumerateSubNodes(ref serviceIndex);

                node.Right = serviceIndex++;
            }
        }

        /// <summary>
        /// Gets subnode with given id.
        /// </summary>
        /// <param name="id">Node id.</param>
        /// <returns>Return null, if there is no node with given id in the hierarchy</returns>
        public HierarchyNode<T> GetNodeById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            return this.FirstOrDefault(n => n.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerator<HierarchyNode<T>> GetEnumerator()
        {
            return _subNodes.Concat(_subNodes.SelectMany(n => n)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
