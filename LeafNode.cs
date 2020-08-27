namespace IrregularZ
{
    public sealed class LeafNode : Node
    {
        public IGeometry Geometry;

        public override void UpdateBounds()
        {
            CompoundBounds.Transform(Geometry.BoundingBox, ref WorldTransform);
        }

        public override void Render(IVisualizer visualizer)
        {
            visualizer.WorldMatrix = WorldTransform;
            Geometry.Render(visualizer);
        }
    }
}