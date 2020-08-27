namespace IrregularZ
{
    public sealed class Scene
    {
        private readonly Node _root;

        public Scene(Node root)
        {
            _root = root;
        }

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

        public void Render(IVisualizer visualizer, Frustum frustum)
        {
            visualizer.Frustum = frustum;
            _root.TraverseDown(node =>
            {
                var containment = node.ContainedBy(frustum);
                if (containment == Containment.Outside) return false;
                visualizer.Clipping = containment == Containment.Partial;
                node.Render(visualizer);
                return true;
            });
        }
    }
}