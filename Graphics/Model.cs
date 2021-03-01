using System.Collections.Generic;
using System.Drawing;

namespace IrregularZ.Graphics
{
    public sealed class Model : IRenderable
    {
        private readonly Dictionary<Color, Mesh> _parts;

        public Model(Dictionary<Color, Mesh> parts)
        {
            _parts = parts;
            foreach (var mesh in _parts.Values) Bounds.Aggregate(mesh.Bounds);
        }

        public Aabb Bounds { get; } = new Aabb();

        public void Render(IRenderer renderer)
        {
            foreach (var (material, mesh) in _parts)
            {
                renderer.Material = material;
                mesh.Render(renderer);
            }
        }
    }
}