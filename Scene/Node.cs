using System;
using IrregularZ.Graphics;

namespace IrregularZ.Scene
{
    public abstract class Node
    {
        internal static readonly BranchNode OrphanParent = new BranchNode();

        protected internal readonly Aabb Bounds;
        public Matrix4x4 LocalTransform;

        internal BranchNode Parent = OrphanParent;
        internal Matrix4x4 WorldTransform;

        protected Node()
        {
            Bounds = new Aabb();
            LocalTransform = Matrix4x4.Identity;
            WorldTransform = Matrix4x4.Identity;
        }

        public Containment ContainedBy(Frustum frustum) => frustum.Evaluate(Bounds);

        public void UpdateTransform() => WorldTransform = Parent.WorldTransform * LocalTransform;

        public virtual void TraverseUp(Func<Node, bool> visitor) => visitor(this);

        public virtual void TraverseDown(Func<Node, bool> visitor) => visitor(this);

        public abstract void UpdateBounds();

        public virtual void Update(double seconds) { }

        public virtual void Render(IRenderer renderer) { }
    }
}