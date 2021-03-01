using System;
using System.Runtime.CompilerServices;
using IrregularZ.Graphics;

namespace IrregularZ.Scene
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public Containment ContainedBy(Frustum frustum) => frustum.Evaluate(Bounds);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void UpdateTransform() => WorldTransform = Parent.WorldTransform * LocalTransform;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public virtual void TraverseUp(Func<Node, bool> visitor) => visitor(this);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public virtual void TraverseDown(Func<Node, bool> visitor) => visitor(this);

        public abstract void UpdateBounds();

        public virtual void Update(double seconds) { }

        public virtual void Render(IRenderer renderer) { }
    }
}