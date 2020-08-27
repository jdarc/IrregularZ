namespace IrregularZ
{
    public interface IGeometry : IRenderable
    {
        int VertexCount { get; }
        int TriangleCount { get; }
        Box BoundingBox { get; }
    }
}