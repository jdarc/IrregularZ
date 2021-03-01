namespace IrregularZ.Graphics
{
    public interface IRenderable
    {
        Aabb Bounds { get; }
        void Render(IRenderer renderer);
    }
}