using System;
using System.Collections;
using System.Collections.Generic;

namespace IrregularZ
{
    public class BranchNode : Node
    {
        private readonly NodeCollection _nodeCollection;

        public BranchNode()
        {
            _nodeCollection = new NodeCollection(this);
        }

        public ICollection<Node> Nodes => _nodeCollection;

        public override void UpdateBounds()
        {
            CompoundBounds.Reset();
            foreach (var node in _nodeCollection) CompoundBounds.Aggregate(node.CompoundBounds);
        }

        public override void TraverseUp(Func<Node, bool> visitor)
        {
            _nodeCollection.TraverseUp(visitor);
            visitor(this);
        }

        public override void TraverseDown(Func<Node, bool> visitor)
        {
            if (visitor(this)) _nodeCollection.TraverseDown(visitor);
        }

        private sealed class NodeCollection : ICollection<Node>
        {
            private readonly List<Node> _childNodes;
            private readonly Node _node;

            public NodeCollection(Node node)
            {
                _node = node;
                _childNodes = new List<Node>();
            }

            public void Add(Node node)
            {
                if (node != null && node.Parent != _node)
                {
                    if (node.Parent is BranchNode parentBranchNode)
                        parentBranchNode.Nodes.Remove(node);

                    node.Parent = _node;
                    _childNodes.Add(node);
                }
            }

            public void Clear()
            {
                var nodes = new Node[Count];
                CopyTo(nodes, 0);
                Array.ForEach(nodes, node => Remove(node));
            }

            public bool Contains(Node item)
            {
                return _childNodes.Contains(item);
            }

            public void CopyTo(Node[] array, int arrayIndex)
            {
                _childNodes.CopyTo(array, arrayIndex);
            }

            public bool Remove(Node node)
            {
                if (node == null || node.Parent != _node) return false;
                node.Parent = OrphanParent;
                _childNodes.Remove(node);
                return true;
            }

            public int Count => _childNodes.Count;

            public bool IsReadOnly => false;

            public IEnumerator<Node> GetEnumerator()
            {
                return _childNodes.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void TraverseUp(Func<Node, bool> visitor)
            {
                _childNodes.ForEach(node => node.TraverseUp(visitor));
            }

            public void TraverseDown(Func<Node, bool> visitor)
            {
                _childNodes.ForEach(node => node.TraverseDown(visitor));
            }
        }
    }
}