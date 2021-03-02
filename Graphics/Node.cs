using System;

namespace IrregularZ.Graphics
{
    public abstract class Node
    {
        internal static readonly BranchNode OrphanParent = new BranchNode();

        protected internal readonly Aabb Bounds;
        public Matrix4 LocalTransform;

        internal BranchNode Parent = OrphanParent;
        internal Matrix4 WorldTransform;

        protected Node()
        {
            Bounds = new Aabb();
            LocalTransform = Matrix4.Identity;
            WorldTransform = Matrix4.Identity;
        }

        public Containment ContainedBy(Frustum frustum) => frustum.Evaluate(Bounds);

        public void UpdateTransform() => WorldTransform = Parent.WorldTransform * LocalTransform;

        public virtual void TraverseUp(Func<Node, bool> visitor) => visitor(this);

        public virtual void TraverseDown(Func<Node, bool> visitor) => visitor(this);

        public abstract void UpdateBounds();

        public virtual void Update(double seconds)
        {
        }

        public virtual void Render(IRenderer renderer)
        {
        }
    }
}