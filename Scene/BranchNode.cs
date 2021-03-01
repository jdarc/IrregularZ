using System;
using System.Collections;
using System.Collections.Generic;

namespace IrregularZ.Scene
{
    public class BranchNode : Node
    {
        private readonly NodeCollection _nodes;

        public BranchNode() => _nodes = new NodeCollection(this);

        public ICollection<Node> Nodes => _nodes;

        public override void UpdateBounds()
        {
            Bounds.Reset();
            foreach (var node in _nodes) Bounds.Aggregate(node.Bounds);
        }
        
        public override void TraverseUp(Func<Node, bool> visitor)
        {
            _nodes.TraverseUp(visitor);
            visitor(this);
        }

        public override void TraverseDown(Func<Node, bool> visitor)
        {
            if (visitor(this)) _nodes.TraverseDown(visitor);
        }

        private sealed class NodeCollection : ICollection<Node>
        {
            private readonly List<Node> _childNodes;
            private readonly BranchNode _node;

            public NodeCollection(BranchNode node)
            {
                _node = node;
                _childNodes = new List<Node>();
            }

            public int Count => _childNodes.Count;

            public bool IsReadOnly => false;

            public void Clear()
            {
                var nodes = new Node[Count];
                CopyTo(nodes, 0);
                foreach (var node in nodes) Remove(node);
            }

            public void CopyTo(Node[] array, int arrayIndex) => _childNodes.CopyTo(array, arrayIndex);

            public bool Contains(Node item) => _childNodes.Contains(item);

            public void Add(Node node)
            {
                if (node == null || node.Parent == _node) return;
                node.Parent.Nodes.Remove(node);
                node.Parent = _node;
                _childNodes.Add(node);
            }

            public bool Remove(Node node)
            {
                if (node == null || node.Parent != _node) return false;
                node.Parent = OrphanParent;
                _childNodes.Remove(node);
                return true;
            }

            public IEnumerator<Node> GetEnumerator() => _childNodes.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void TraverseUp(Func<Node, bool> visitor) => _childNodes.ForEach(node => node.TraverseUp(visitor));

            public void TraverseDown(Func<Node, bool> visitor) => _childNodes.ForEach(node => node.TraverseDown(visitor));
        }
    }
}