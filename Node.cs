using System;

namespace IrregularZ
{
    public class Node
    {
        internal static readonly Node OrphanParent = new Node();
        internal Box CompoundBounds;
        public Matrix4F LocalTransform;

        internal Node Parent = OrphanParent;
        internal Matrix4F WorldTransform;

        protected Node()
        {
            CompoundBounds = new Box();
            LocalTransform = Matrix4F.Identity;
            WorldTransform = Matrix4F.Identity;
        }

        public Containment ContainedBy(Frustum frustum)
        {
            return frustum.Evaluate(CompoundBounds);
        }

        public void UpdateTransform()
        {
            WorldTransform.Multiply(ref Parent.WorldTransform, ref LocalTransform);
        }

        public virtual void TraverseUp(Func<Node, bool> visitor)
        {
            visitor(this);
        }

        public virtual void TraverseDown(Func<Node, bool> visitor)
        {
            visitor(this);
        }

        public virtual void UpdateBounds()
        {
        }

        public virtual void Update(double seconds)
        {
        }

        public virtual void Render(IVisualizer visualizer)
        {
        }
    }
}