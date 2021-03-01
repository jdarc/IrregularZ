namespace IrregularZ.Graphics
{
    public sealed class Mesh : IRenderable
    {
        private readonly int[] _indices;
        private readonly float[] _vertices;

        public Mesh(float[] vertices, int[] indices)
        {
            _vertices = vertices;
            _indices = indices;
            Bounds = new Aabb();
            for (var i = 0; i < vertices.Length; i += 3)
                Bounds.Aggregate(vertices[i], vertices[i + 1], vertices[i + 2]);
        }

        public Aabb Bounds { get; }

        public void Render(IRenderer renderer)
        {
            renderer.Render(_vertices, _indices);
        }
    }
}