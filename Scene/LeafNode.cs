using IrregularZ.Graphics;

namespace IrregularZ.Scene
{
    public sealed class LeafNode : Node
    {
        public IRenderable Geometry;

        public override void UpdateBounds() => Bounds.Aggregate(Geometry.Bounds, WorldTransform);
        
        public override void Render(IRenderer renderer)
        {
            renderer.WorldMatrix = WorldTransform;
            Geometry.Render(renderer);
        }
    }
}
