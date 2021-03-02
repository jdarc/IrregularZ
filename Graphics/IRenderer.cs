using System.Drawing;

namespace IrregularZ.Graphics
{
    public interface IRenderer
    {
        Color Material { get; set; }

        Matrix4 WorldMatrix { get; set; }

        Clipper Clipper { get; set; }

        void Render(float[] vertexBuffer, int[] indexBuffer);
    }
}