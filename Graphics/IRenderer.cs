using System.Drawing;

namespace IrregularZ.Graphics
{
    public interface IRenderer
    {
        Color Material { get; set; }

        Matrix4x4 WorldMatrix { get; set; }
        Matrix4x4 ViewMatrix { get; set; }
        Matrix4x4 ProjectionMatrix { get; set; }

        Frustum Frustum { get; set; }
        bool Clipping { get; set; }


        void Render(float[] vertexBuffer, int[] indexBuffer);
    }
}