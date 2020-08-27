using System;

namespace IrregularZ
{
    public interface IVisualizer : IDisposable
    {
        Matrix4F WorldMatrix { get; set; }
        Matrix4F ViewMatrix { get; set; }
        Matrix4F ProjectionMatrix { get; set; }
        Matrix4F CombinedMatrix { get; }

        Frustum Frustum { get; set; }
        bool Clipping { get; set; }

        void Draw(Mesh mesh, Material material);
    }
}