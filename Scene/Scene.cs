﻿using System.Runtime.CompilerServices;
using IrregularZ.Graphics;

namespace IrregularZ.Scene
{
    public sealed class Scene
    {
        private readonly Node _root;

        public Scene(Node root) => _root = root;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Update(double seconds)
        {
            _root.TraverseDown(node =>
            {
                node.Update(seconds);
                node.UpdateTransform();
                return true;
            });

            _root.TraverseUp(node =>
            {
                node.UpdateBounds();
                return true;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Render(IRenderer renderer, Frustum frustum)
        {
            renderer.Clipper = new Clipper(frustum);
            _root.TraverseDown(node =>
            {
                var containment = node.ContainedBy(frustum);
                if (containment == Containment.Outside) return false;
                renderer.Clipper.Enabled = containment == Containment.Partial;
                node.Render(renderer);
                return true;
            });
        }
    }
}