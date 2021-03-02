using System;
using System.Drawing;

namespace IrregularZ.Graphics
{
    public sealed class Renderer : IRenderer
    {
        private readonly FrameBuffer<int> _colorRaster;
        private readonly FrameBuffer<float> _depthBuffer;
        private Vector3 _lightDir;

        public Renderer(FrameBuffer<int> colorRaster, FrameBuffer<float> depthBuffer)
        {
            _colorRaster = colorRaster;
            _depthBuffer = depthBuffer;
            ViewportMatrix = Matrix4.CreateViewportMatrix(colorRaster.Width, colorRaster.Height);
            _lightDir = new Vector3(0, -1, 0);
        }

        public Color Material { get; set; }

        public Clipper Clipper { get; set; }

        public Matrix4 WorldMatrix { get; set; }

        public Matrix4 ViewMatrix { get; set; }

        public Matrix4 ProjectionMatrix { get; set; }

        public readonly Matrix4 ViewportMatrix;

        public void MoveLight(float x, float y, float z) => _lightDir = Vector3.Normalize(new Vector3(-x, -y, -z));

        public void Clear(int rgb)
        {
            _colorRaster.Fill(0xFF << 0x18 | ((rgb << 16) & 0xFF0000) | (rgb & 0xFF00) | ((rgb >> 16) & 0xFF));
            _depthBuffer.Fill(1F);
        }

        public void Render(float[] vertexBuffer, int[] indexBuffer)
        {
            var matrix = ViewportMatrix * ProjectionMatrix * ViewMatrix;
            for (var i = 0; i < indexBuffer.Length; i += 3)
            {
                var i0 = indexBuffer[i + 0] * 3;
                var v0 = WorldMatrix * new Vector3(vertexBuffer[i0 + 0], vertexBuffer[i0 + 1], vertexBuffer[i0 + 2]);

                var i1 = indexBuffer[i + 1] * 3;
                var v1 = WorldMatrix * new Vector3(vertexBuffer[i1 + 0], vertexBuffer[i1 + 1], vertexBuffer[i1 + 2]);

                var i2 = indexBuffer[i + 2] * 3;
                var v2 = WorldMatrix * new Vector3(vertexBuffer[i2 + 0], vertexBuffer[i2 + 1], vertexBuffer[i2 + 2]);

                Render(matrix, v0, v1, v2, ComputeColor(127, ComputeIllumination(_lightDir, v0, v1, v2), Material));
            }
        }

        private void Render(in Matrix4 matrix, in Vector3 v0, in Vector3 v1, in Vector3 v2, int color)
        {
            var count = Clipper.Clip(v0, v1, v2);
            if (count < 3) return;
            var result = Clipper.Result;
            if (count < 4)
            {
                ScanOrder(Project(matrix, result[0]), Project(matrix, result[1]), Project(matrix, result[2]), color);
                return;
            }

            var p0 = Project(matrix, result[0]);
            var p2 = Project(matrix, result[1]);
            var n = 2;
            do
            {
                var p1 = p2;
                p2 = Project(matrix, result[n]);
                ScanOrder(p0, p1, p2, color);
            } while (++n < count);
        }

        private void ScanOrder(in Vector3 v0, in Vector3 v1, in Vector3 v2, int color)
        {
            var gradients = new Gradients(v0, v1, v2);
            if (v0.Y < v1.Y)
            {
                if (v2.Y < v0.Y) Scan(gradients, v2, v0, v1, false, color);
                else if (v1.Y < v2.Y) Scan(gradients, v0, v1, v2, false, color);
                else Scan(gradients, v0, v2, v1, true, color);
            }
            else
            {
                if (v2.Y < v1.Y) Scan(gradients, v2, v1, v0, true, color);
                else if (v0.Y < v2.Y) Scan(gradients, v1, v0, v2, true, color);
                else Scan(gradients, v1, v2, v0, false, color);
            }
        }

        private void Scan(in Gradients gradients, in Vector3 v0, in Vector3 v1, in Vector3 v2, bool swap, int color)
        {
            var edgeA = new Edge();
            if (edgeA.Configure(v0, v2, gradients) <= 0) return;

            var edgeB = new Edge();
            if (edgeB.Configure(v0, v1, gradients) > 0)
                if (swap) Rasterize(gradients, ref edgeB, ref edgeA, edgeB, color);
                else Rasterize(gradients, ref edgeA, ref edgeB, edgeB, color);

            if (edgeB.Configure(v1, v2, gradients) <= 0) return;
            if (swap) Rasterize(gradients, ref edgeB, ref edgeA, edgeB, color);
            else Rasterize(gradients, ref edgeA, ref edgeB, edgeB, color);
        }

        private void Rasterize(in Gradients gradients, ref Edge left, ref Edge right, in Edge leader, int color)
        {
            var offset = leader.Y * _colorRaster.Width;
            var height = leader.Height;
            while (height-- > 0)
            {
                var start = (int) MathF.Ceiling(left.X);
                var width = (int) MathF.Ceiling(right.X) - start;
                if (width > 0)
                {
                    var mem = offset + start;
                    var w = left.Z + (start - left.X) * gradients.dZdX;
                    while (width-- > 0)
                    {
                        if (w < _depthBuffer[mem])
                        {
                            _depthBuffer[mem] = w;
                            _colorRaster[mem] = color;
                        }

                        w += gradients.dZdX;
                        ++mem;
                    }
                }

                left.X += left.XStep;
                left.Z += left.ZStep;
                right.X += right.XStep;
                offset += _colorRaster.Width;
            }
        }

        private static Vector3 Project(in Matrix4 m, in Vector3 v)
        {
            var x = m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14;
            var y = m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24;
            var z = m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34;
            var w = m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44;
            return new Vector3(x / w, y / w, z / w);
        }

        private static int ComputeColor(int ka, int kd, Color diffuse)
        {
            var blu = Math.Clamp((ka * diffuse.B + kd * diffuse.B) >> 8, 0, 255) << 0x10;
            var grn = Math.Clamp((ka * diffuse.G + kd * diffuse.G) >> 8, 0, 255) << 0x08;
            var red = Math.Clamp((ka * diffuse.R + kd * diffuse.R) >> 8, 0, 255);
            return -0x1000000 | blu | grn | red;
        }

        private static int ComputeIllumination(Vector3 toLight, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            var nx = (v0.Y - v1.Y) * (v2.Z - v1.Z) - (v0.Z - v1.Z) * (v2.Y - v1.Y);
            var ny = (v0.Z - v1.Z) * (v2.X - v1.X) - (v0.X - v1.X) * (v2.Z - v1.Z);
            var nz = (v0.X - v1.X) * (v2.Y - v1.Y) - (v0.Y - v1.Y) * (v2.X - v1.X);
            var dot = (toLight.X * nx + toLight.Y * ny + toLight.Z * nz) / Math.Sqrt(nx * nx + ny * ny + nz * nz);
            return (int) (Math.Clamp(dot, 0F, 1F) * 255);
        }
    }
}