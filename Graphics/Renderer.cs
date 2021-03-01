using System;
using System.Drawing;

namespace IrregularZ.Graphics
{
    public sealed class Renderer : IRenderer
    {
        private const int Opaque = 0xFF << 0x18;

        private readonly Tuple3[] _clipBufferA = new Tuple3[8];
        private readonly Tuple3[] _clipBufferB = new Tuple3[8];
        private readonly FrameBuffer<int> _colorRaster;
        private readonly FrameBuffer<float> _depthBuffer;

        public readonly Matrix4x4 ViewportMatrix;

        private Matrix4x4 _combMatrix;
        private Vector3 _lightDir;

        public Renderer(FrameBuffer<int> colorRaster, FrameBuffer<float> depthBuffer)
        {
            _colorRaster = colorRaster;
            _depthBuffer = depthBuffer;
            ViewportMatrix = Matrix4x4.CreateViewportMatrix(_colorRaster.Width, _colorRaster.Height);
            _lightDir = new Vector3(0, -1, 0);
            Clipping = true;
        }

        public bool Clipping { get; set; }

        public Color Material { get; set; }

        public Frustum Frustum { get; set; }
        
        public Matrix4x4 WorldMatrix { get; set; }

        public Matrix4x4 ViewMatrix { get; set; }

        public Matrix4x4 ProjectionMatrix { get; set; }

        public void Render(float[] vertexBuffer, int[] indexBuffer)
        {
            unsafe
            {
                _combMatrix = ViewportMatrix * ProjectionMatrix * ViewMatrix;

                fixed (Tuple3* clpa = &_clipBufferA[0], clpb = &_clipBufferB[0])
                {
                    for (var i = 0; i < indexBuffer.Length; i += 3)
                    {
                        var s0X = vertexBuffer[indexBuffer[i] * 3 + 0];
                        var s0Y = vertexBuffer[indexBuffer[i] * 3 + 1];
                        var s0Z = vertexBuffer[indexBuffer[i] * 3 + 2];
                        var s1X = vertexBuffer[indexBuffer[i + 1] * 3 + 0];
                        var s1Y = vertexBuffer[indexBuffer[i + 1] * 3 + 1];
                        var s1Z = vertexBuffer[indexBuffer[i + 1] * 3 + 2];
                        var s2X = vertexBuffer[indexBuffer[i + 2] * 3 + 0];
                        var s2Y = vertexBuffer[indexBuffer[i + 2] * 3 + 1];
                        var s2Z = vertexBuffer[indexBuffer[i + 2] * 3 + 2];
                        var v0 = new Tuple3
                        {
                            X = WorldMatrix.M11 * s0X + WorldMatrix.M12 * s0Y + WorldMatrix.M13 * s0Z + WorldMatrix.M14,
                            Y = WorldMatrix.M21 * s0X + WorldMatrix.M22 * s0Y + WorldMatrix.M23 * s0Z + WorldMatrix.M24,
                            Z = WorldMatrix.M31 * s0X + WorldMatrix.M32 * s0Y + WorldMatrix.M33 * s0Z + WorldMatrix.M34
                        };
                        var v1 = new Tuple3
                        {
                            X = WorldMatrix.M11 * s1X + WorldMatrix.M12 * s1Y + WorldMatrix.M13 * s1Z + WorldMatrix.M14,
                            Y = WorldMatrix.M21 * s1X + WorldMatrix.M22 * s1Y + WorldMatrix.M23 * s1Z + WorldMatrix.M24,
                            Z = WorldMatrix.M31 * s1X + WorldMatrix.M32 * s1Y + WorldMatrix.M33 * s1Z + WorldMatrix.M34
                        };
                        var v2 = new Tuple3
                        {
                            X = WorldMatrix.M11 * s2X + WorldMatrix.M12 * s2Y + WorldMatrix.M13 * s2Z + WorldMatrix.M14,
                            Y = WorldMatrix.M21 * s2X + WorldMatrix.M22 * s2Y + WorldMatrix.M23 * s2Z + WorldMatrix.M24,
                            Z = WorldMatrix.M31 * s2X + WorldMatrix.M32 * s2Y + WorldMatrix.M33 * s2Z + WorldMatrix.M34
                        };
                        var col = ComputeColor(127, ComputeIllumination(_lightDir, v0, v1, v2), Material);
                        ClipAndDraw(Frustum, ref v0, ref v1, ref v2, clpa, clpb, col);
                    }
                }
            }
        }

        public void MoveLight(float x, float y, float z)
        {
            _lightDir = Vector3.Normalize(new Vector3(-x, -y, -z));
        }

        public void Clear(int rgb, float depth)
        {
            _colorRaster.Fill(Opaque | ((rgb << 16) & 0xFF0000) | (rgb & 0xFF00) | ((rgb >> 16) & 0xFF));
            _depthBuffer.Fill(depth);
        }

        private unsafe void ClipAndDraw(Frustum frustum, ref Tuple3 v0, ref Tuple3 v1, ref Tuple3 v2, Tuple3* cba, Tuple3* cbb, int color)
        {
            Tuple3 s0;
            Tuple3 s1;
            Tuple3 s2;

            byte clipMask = 0;
            if (Clipping)
            {
                int i;
                var planes = &frustum.Near;
                byte mask = 1;
                for (i = 0; i < 6; ++i)
                {
                    var p = 0;
                    var pl = planes + i;
                    if (pl->Normal.X * v0.X + pl->Normal.Y * v0.Y + pl->Normal.Z * v0.Z - pl->Distance > 0)
                    {
                        ++p;
                        clipMask |= mask;
                    }

                    if (pl->Normal.X * v1.X + pl->Normal.Y * v1.Y + pl->Normal.Z * v1.Z - pl->Distance > 0)
                    {
                        ++p;
                        clipMask |= mask;
                    }

                    if (pl->Normal.X * v2.X + pl->Normal.Y * v2.Y + pl->Normal.Z * v2.Z - pl->Distance > 0)
                    {
                        if (++p == 3) return;
                        clipMask |= mask;
                    }

                    mask <<= 1;
                }

                if (clipMask != 0)
                {
                    var a = cba;
                    var b = cbb;
                    var acount = 3;
                    var bcount = 0;

                    a[0] = a[3] = v0;
                    a[1] = v1;
                    a[2] = v2;

                    mask = 1;
                    for (i = 0; i < 6; ++i)
                    {
                        if ((clipMask & mask) == mask)
                        {
                            var pl = planes + i;
                            var pln = &pl->Normal;
                            var pld = pl->Distance;
                            var a1 = a;
                            var f1 = pln->X * a1->X + pln->Y * a1->Y + pln->Z * a1->Z;
                            for (var v = 1; v <= acount; ++v)
                            {
                                var a2 = a + v;
                                var f2 = pln->X * a2->X + pln->Y * a2->Y + pln->Z * a2->Z;
                                var dot = -f1 + pld;
                                if (dot > 0)
                                {
                                    if (f2 - pld < 0)
                                    {
                                        *(b + bcount++) = *a2;
                                    }
                                    else
                                    {
                                        dot /= f2 - f1;
                                        var target = b + bcount++;
                                        target->X = a1->X + (a2->X - a1->X) * dot;
                                        target->Y = a1->Y + (a2->Y - a1->Y) * dot;
                                        target->Z = a1->Z + (a2->Z - a1->Z) * dot;
                                    }
                                }
                                else
                                {
                                    if (f2 - pld < 0)
                                    {
                                        dot /= f2 - f1;
                                        var target = b + bcount++;
                                        target->X = a1->X + (a2->X - a1->X) * dot;
                                        target->Y = a1->Y + (a2->Y - a1->Y) * dot;
                                        target->Z = a1->Z + (a2->Z - a1->Z) * dot;
                                        *(b + bcount++) = *a2;
                                    }
                                }

                                a1 = a2;
                                f1 = f2;
                            }

                            if (bcount < 3) return;

                            *(b + bcount) = *b;
                            acount = bcount;
                            bcount = 0;
                            var t = a;
                            a = b;
                            b = t;
                        }

                        mask <<= 1;
                    }

                    if (acount > 3)
                    {
                        acount -= 2;
                        var ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                                       _combMatrix.M44);
                        s0.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                                     _combMatrix.M14);
                        s0.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                                     _combMatrix.M24);
                        s0.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                                     _combMatrix.M34);
                        ++a;
                        ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                                   _combMatrix.M44);
                        s2.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                                     _combMatrix.M14);
                        s2.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                                     _combMatrix.M24);
                        s2.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                                     _combMatrix.M34);
                        do
                        {
                            s1 = s2;
                            ++a;
                            ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                                       _combMatrix.M44);
                            s2.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                                         _combMatrix.M14);
                            s2.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                                         _combMatrix.M24);
                            s2.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                                         _combMatrix.M34);
                            ScanOrder(color, ref s0, ref s1, ref s2);
                        } while (--acount > 0);

                        return;
                    }

                    v0 = *a;
                    v1 = *++a;
                    v2 = *++a;
                }
            }

            var ow0 = 1F / (_combMatrix.M41 * v0.X + _combMatrix.M42 * v0.Y + _combMatrix.M43 * v0.Z + _combMatrix.M44);
            s0.X = ow0 * (_combMatrix.M11 * v0.X + _combMatrix.M12 * v0.Y + _combMatrix.M13 * v0.Z + _combMatrix.M14);
            s0.Y = ow0 * (_combMatrix.M21 * v0.X + _combMatrix.M22 * v0.Y + _combMatrix.M23 * v0.Z + _combMatrix.M24);
            s0.Z = ow0 * (_combMatrix.M31 * v0.X + _combMatrix.M32 * v0.Y + _combMatrix.M33 * v0.Z + _combMatrix.M34);

            var ow1 = 1F / (_combMatrix.M41 * v1.X + _combMatrix.M42 * v1.Y + _combMatrix.M43 * v1.Z + _combMatrix.M44);
            s1.X = ow1 * (_combMatrix.M11 * v1.X + _combMatrix.M12 * v1.Y + _combMatrix.M13 * v1.Z + _combMatrix.M14);
            s1.Y = ow1 * (_combMatrix.M21 * v1.X + _combMatrix.M22 * v1.Y + _combMatrix.M23 * v1.Z + _combMatrix.M24);
            s1.Z = ow1 * (_combMatrix.M31 * v1.X + _combMatrix.M32 * v1.Y + _combMatrix.M33 * v1.Z + _combMatrix.M34);

            var ow2 = 1F / (_combMatrix.M41 * v2.X + _combMatrix.M42 * v2.Y + _combMatrix.M43 * v2.Z + _combMatrix.M44);
            s2.X = ow2 * (_combMatrix.M11 * v2.X + _combMatrix.M12 * v2.Y + _combMatrix.M13 * v2.Z + _combMatrix.M14);
            s2.Y = ow2 * (_combMatrix.M21 * v2.X + _combMatrix.M22 * v2.Y + _combMatrix.M23 * v2.Z + _combMatrix.M24);
            s2.Z = ow2 * (_combMatrix.M31 * v2.X + _combMatrix.M32 * v2.Y + _combMatrix.M33 * v2.Z + _combMatrix.M34);

            ScanOrder(color, ref s0, ref s1, ref s2);
        }

        private void ScanOrder(int color, ref Tuple3 s0, ref Tuple3 s1, ref Tuple3 s2)
        {
            var gradients = new Gradients(ref s0, ref s1, ref s2);
            if (s0.Y < s1.Y)
            {
                if (s2.Y < s0.Y) Scan(ref gradients, ref s2, ref s0, ref s1, false, color);
                else if (s1.Y < s2.Y) Scan(ref gradients, ref s0, ref s1, ref s2, false, color);
                else Scan(ref gradients, ref s0, ref s2, ref s1, true, color);
            }
            else
            {
                if (s2.Y < s1.Y) Scan(ref gradients, ref s2, ref s1, ref s0, true, color);
                else if (s0.Y < s2.Y) Scan(ref gradients, ref s1, ref s0, ref s2, true, color);
                else Scan(ref gradients, ref s1, ref s2, ref s0, false, color);
            }
        }

        private void Scan(ref Gradients gradients, ref Tuple3 s0, ref Tuple3 s1, ref Tuple3 s2, bool swap, int color)
        {
            var edgeA = new Edge();
            if (edgeA.Configure(ref s0, ref s2, ref gradients) <= 0) return;

            var edgeB = new Edge();
            if (edgeB.Configure(ref s0, ref s1, ref gradients) > 0)
                if (swap) Rasterize(ref gradients, ref edgeB, ref edgeA, ref edgeB, color);
                else Rasterize(ref gradients, ref edgeA, ref edgeB, ref edgeB, color);

            if (edgeB.Configure(ref s1, ref s2, ref gradients) <= 0) return;
            if (swap) Rasterize(ref gradients, ref edgeB, ref edgeA, ref edgeB, color);
            else Rasterize(ref gradients, ref edgeA, ref edgeB, ref edgeB, color);
        }

        private void Rasterize(ref Gradients gradients, ref Edge left, ref Edge right, ref Edge leader, int color)
        {
            var offset = leader.y * _colorRaster.Width;
            var height = leader.height;
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

        private static int ComputeColor(int ka, int kd, Color diffuse)
        {
            var blu = Math.Clamp((ka * diffuse.B + kd * diffuse.B) >> 8, 0, 255) << 0x10;
            var grn = Math.Clamp((ka * diffuse.G + kd * diffuse.G) >> 8, 0, 255) << 0x08;
            var red = Math.Clamp((ka * diffuse.R + kd * diffuse.R) >> 8, 0, 255);
            return Opaque | blu | grn | red;
        }

        private static int ComputeIllumination(Vector3 toLight, Tuple3 v0, Tuple3 v1, Tuple3 v2)
        {
            var nx = (v0.Y - v1.Y) * (v2.Z - v1.Z) - (v0.Z - v1.Z) * (v2.Y - v1.Y);
            var ny = (v0.Z - v1.Z) * (v2.X - v1.X) - (v0.X - v1.X) * (v2.Z - v1.Z);
            var nz = (v0.X - v1.X) * (v2.Y - v1.Y) - (v0.Y - v1.Y) * (v2.X - v1.X);
            var dot = (toLight.X * nx + toLight.Y * ny + toLight.Z * nz) / Math.Sqrt(nx * nx + ny * ny + nz * nz);
            return (int) (Math.Clamp(dot, 0F, 1F) * 255);
        }
    }
}