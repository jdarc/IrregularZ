using System;

namespace IrregularZ
{
    public sealed class Visualizer : IVisualizer
    {
        private const int Opaque = 0xFF << 0x18;

        private readonly Vector3F[] _clipBufferA = new Vector3F[16];
        private readonly Vector3F[] _clipBufferB = new Vector3F[16];
        private readonly Raster _colorRaster;
        private readonly Raster _depthRaster;

        private Vector3F _camera;
        private unsafe int* _cbPtr;
        private Matrix4F _combMatrix;
        private bool _dirty;
        private Vector3F _lightDir;
        private Matrix4F _projMatrix;

        private Vector3F[] _transformed = new Vector3F[16];

        private Matrix4F _viewMatrix;
        private Matrix4F _viewportMatrix;

        private unsafe float* _zbPtr;

        public Visualizer(Raster colorRaster, Raster depthRaster)
        {
            _colorRaster = colorRaster;
            _depthRaster = depthRaster;
            _viewportMatrix = new Matrix4F(_colorRaster.Width * 0.5f, 0, 0, _colorRaster.Width * 0.5f - 0.5f, 0,
                _colorRaster.Height * 0.5f, 0, _colorRaster.Height * 0.5f - 0.5f, 0, 0, 1, 0, 0, 0, 0, 1);
            _lightDir = new Vector3F(0, -1, 0);
            _dirty = true;
            Clipping = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Frustum Frustum { get; set; }

        public bool Clipping { get; set; }

        public Matrix4F WorldMatrix { get; set; }

        public Matrix4F ViewMatrix
        {
            get => _viewMatrix;
            set
            {
                _viewMatrix = value;
                _camera.X = -value.E11 * value.E14 - value.E21 * value.E24 - value.E31 * value.E34;
                _camera.Y = -value.E12 * value.E14 - value.E22 * value.E24 - value.E32 * value.E34;
                _camera.Z = -value.E13 * value.E14 - value.E23 * value.E24 - value.E33 * value.E34;
                _dirty = true;
            }
        }

        public Matrix4F ProjectionMatrix
        {
            get => _projMatrix;
            set
            {
                _projMatrix = value;
                _dirty = true;
            }
        }

        public Matrix4F CombinedMatrix => _combMatrix;

        public void Draw(Mesh mesh, Material material)
        {
            Draw(mesh.Vertices, mesh.Indices, material);
        }

        public void Clear(int rgb, float depth)
        {
            _colorRaster.Clear((0xFF << 24) | rgb);
            _depthRaster.Buffer.Fill(depth);
        }

        public unsafe void Draw(Vector3F[] vertexBuffer, int[] indexBuffer, Material material)
        {
            _zbPtr = (float*) _depthRaster.Buffer.Scan0;
            _cbPtr = (int*) _colorRaster.Buffer.Scan0;
            var frustum = Frustum;
            var e11 = WorldMatrix.E11;
            var e12 = WorldMatrix.E12;
            var e13 = WorldMatrix.E13;
            var e14 = WorldMatrix.E14;
            var e21 = WorldMatrix.E21;
            var e22 = WorldMatrix.E22;
            var e23 = WorldMatrix.E23;
            var e24 = WorldMatrix.E24;
            var e31 = WorldMatrix.E31;
            var e32 = WorldMatrix.E32;
            var e33 = WorldMatrix.E33;
            var e34 = WorldMatrix.E34;
            var cax = _camera.X;
            var cay = _camera.Y;
            var caz = _camera.Z;
            var lx = _lightDir.X;
            var ly = _lightDir.Y;
            var lz = _lightDir.Z;
            var mats = new Mats(material);
            var kaR = mats.KaR;
            var kdR = mats.KdR;
            var kaG = mats.KaG;
            var kdG = mats.KdG;
            var kaB = mats.KaB;
            var kdB = mats.KdB;

            if (_dirty)
            {
                _combMatrix.Multiply(ref _projMatrix, ref _viewMatrix);
                _combMatrix.Multiply(ref _viewportMatrix, ref _combMatrix);
                _dirty = false;
            }

            if (_transformed.Length < vertexBuffer.Length) _transformed = new Vector3F[vertexBuffer.Length];

            fixed (Vector3F* src = vertexBuffer, dst = _transformed, clpa = &_clipBufferA[0], clpb = &_clipBufferB[0])
            {
                var svb = src;
                var dvb = dst;
                for (var k = 0; k < vertexBuffer.Length; ++k)
                {
                    var sx = svb->X;
                    var sy = svb->Y;
                    var sz = svb->Z;
                    ++svb;
                    dvb->X = e11 * sx + e12 * sy + e13 * sz + e14;
                    dvb->Y = e21 * sx + e22 * sy + e23 * sz + e24;
                    dvb->Z = e31 * sx + e32 * sy + e33 * sz + e34;
                    ++dvb;
                }

                fixed (int* idx = indexBuffer)
                {
                    var i = 0;
                    while (i < indexBuffer.Length)
                    {
                        var v0 = dst + idx[i++];
                        var v1 = dst + idx[i++];
                        var v2 = dst + idx[i++];
                        var ax = v0->X - v1->X;
                        var ay = v0->Y - v1->Y;
                        var az = v0->Z - v1->Z;
                        var cx = v2->X - v1->X;
                        var cy = v2->Y - v1->Y;
                        var cz = v2->Z - v1->Z;
                        var nx = ay * cz - az * cy;
                        var ny = az * cx - ax * cz;
                        var nz = ax * cy - ay * cx;
                        if (nx * (cax - v1->X) + ny * (cay - v1->Y) + nz * (caz - v1->Z) < 0)
                        {
                            var len = 1.0 / Math.Sqrt(nx * nx + ny * ny + nz * nz);
                            var lit = (int) (0xFF * Math.Max(0, lx * nx * len + ly * ny * len + lz * nz * len));
                            var red = Function.ClampByte(kaR + ((kdR * lit) >> 8));
                            var grn = Function.ClampByte(kaG + ((kdG * lit) >> 8));
                            var blu = Function.ClampByte(kaB + ((kdB * lit) >> 8));
                            ClipAndDraw(&frustum, v0, v1, v2, clpa, clpb, Opaque | (red << 0x10) | (grn << 0x08) | blu);
                        }
                    }
                }
            }
        }

        ~Visualizer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _colorRaster.Dispose();
                _depthRaster.Dispose();
            }
        }

        public void MoveLight(float x, float y, float z)
        {
            _lightDir.Set(-x, -y, -z);
            _lightDir.Normalize();
        }

        private unsafe void ClipAndDraw(Frustum* frustum, Vector3F* v0, Vector3F* v1, Vector3F* v2, Vector3F* cba,
            Vector3F* cbb, int color)
        {
            Vector3F s0;
            Vector3F s1;
            Vector3F s2;
            var e11 = _combMatrix.E11;
            var e12 = _combMatrix.E12;
            var e13 = _combMatrix.E13;
            var e14 = _combMatrix.E14;
            var e21 = _combMatrix.E21;
            var e22 = _combMatrix.E22;
            var e23 = _combMatrix.E23;
            var e24 = _combMatrix.E24;
            var e31 = _combMatrix.E31;
            var e32 = _combMatrix.E32;
            var e33 = _combMatrix.E33;
            var e34 = _combMatrix.E34;
            var e41 = _combMatrix.E41;
            var e42 = _combMatrix.E42;
            var e43 = _combMatrix.E43;
            var e44 = _combMatrix.E44;

            byte clipmask = 0;
            if (Clipping)
            {
                int i;
                var planes = &frustum->Near;
                byte mask = 1;
                for (i = 0; i < 6; ++i)
                {
                    var p = 0;
                    var pl = planes + i;
                    if (pl->Normal.X * v0->X + pl->Normal.Y * v0->Y + pl->Normal.Z * v0->Z - pl->Distance > 0)
                    {
                        ++p;
                        clipmask |= mask;
                    }

                    if (pl->Normal.X * v1->X + pl->Normal.Y * v1->Y + pl->Normal.Z * v1->Z - pl->Distance > 0)
                    {
                        ++p;
                        clipmask |= mask;
                    }

                    if (pl->Normal.X * v2->X + pl->Normal.Y * v2->Y + pl->Normal.Z * v2->Z - pl->Distance > 0)
                    {
                        if (++p == 3) return;
                        clipmask |= mask;
                    }

                    mask <<= 1;
                }

                if (clipmask != 0)
                {
                    var a = cba;
                    var b = cbb;
                    var acount = 3;
                    var bcount = 0;

                    a[0] = a[3] = *v0;
                    a[1] = *v1;
                    a[2] = *v2;

                    mask = 1;
                    for (i = 0; i < 6; ++i)
                    {
                        if ((clipmask & mask) == mask)
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
                        var ow = 1.0f / (e41 * a->X + e42 * a->Y + e43 * a->Z + e44);
                        s0.X = ow * (e11 * a->X + e12 * a->Y + e13 * a->Z + e14);
                        s0.Y = ow * (e21 * a->X + e22 * a->Y + e23 * a->Z + e24);
                        s0.Z = ow * (e31 * a->X + e32 * a->Y + e33 * a->Z + e34);
                        ++a;
                        ow = 1.0f / (e41 * a->X + e42 * a->Y + e43 * a->Z + e44);
                        s2.X = ow * (e11 * a->X + e12 * a->Y + e13 * a->Z + e14);
                        s2.Y = ow * (e21 * a->X + e22 * a->Y + e23 * a->Z + e24);
                        s2.Z = ow * (e31 * a->X + e32 * a->Y + e33 * a->Z + e34);
                        do
                        {
                            s1 = s2;
                            ++a;
                            ow = 1.0f / (e41 * a->X + e42 * a->Y + e43 * a->Z + e44);
                            s2.X = ow * (e11 * a->X + e12 * a->Y + e13 * a->Z + e14);
                            s2.Y = ow * (e21 * a->X + e22 * a->Y + e23 * a->Z + e24);
                            s2.Z = ow * (e31 * a->X + e32 * a->Y + e33 * a->Z + e34);
                            ScanOrder(color, ref s0, ref s1, ref s2);
                        } while (--acount > 0);

                        return;
                    }

                    v0 = a;
                    v1 = ++a;
                    v2 = ++a;
                }
            }

            var ow0 = 1.0f / (e41 * v0->X + e42 * v0->Y + e43 * v0->Z + e44);
            var ow1 = 1.0f / (e41 * v1->X + e42 * v1->Y + e43 * v1->Z + e44);
            var ow2 = 1.0f / (e41 * v2->X + e42 * v2->Y + e43 * v2->Z + e44);

            s0.X = ow0 * (e11 * v0->X + e12 * v0->Y + e13 * v0->Z + e14);
            s1.X = ow1 * (e11 * v1->X + e12 * v1->Y + e13 * v1->Z + e14);
            s2.X = ow2 * (e11 * v2->X + e12 * v2->Y + e13 * v2->Z + e14);

            s0.Y = ow0 * (e21 * v0->X + e22 * v0->Y + e23 * v0->Z + e24);
            s1.Y = ow1 * (e21 * v1->X + e22 * v1->Y + e23 * v1->Z + e24);
            s2.Y = ow2 * (e21 * v2->X + e22 * v2->Y + e23 * v2->Z + e24);

            s0.Z = ow0 * (e31 * v0->X + e32 * v0->Y + e33 * v0->Z + e34);
            s1.Z = ow1 * (e31 * v1->X + e32 * v1->Y + e33 * v1->Z + e34);
            s2.Z = ow2 * (e31 * v2->X + e32 * v2->Y + e33 * v2->Z + e34);

            ScanOrder(color, ref s0, ref s1, ref s2);
        }

        private void ScanOrder(int color, ref Vector3F s0, ref Vector3F s1, ref Vector3F s2)
        {
            if (s0.Y < s1.Y)
            {
                if (s2.Y < s0.Y)
                {
                    Scan(ref s2, ref s0, ref s1, false, color);
                    return;
                }

                if (s1.Y < s2.Y)
                {
                    Scan(ref s0, ref s1, ref s2, false, color);
                    return;
                }

                Scan(ref s0, ref s2, ref s1, true, color);
                return;
            }

            if (s2.Y < s1.Y)
            {
                Scan(ref s2, ref s1, ref s0, true, color);
                return;
            }

            if (s0.Y < s2.Y)
            {
                Scan(ref s1, ref s0, ref s2, true, color);
                return;
            }

            Scan(ref s1, ref s2, ref s0, false, color);
        }

        private unsafe void Scan(ref Vector3F s0, ref Vector3F s1, ref Vector3F s2, bool swap, int color)
        {
            var y1 = Function.Ceil(s0.Y);
            var height = Function.Ceil(s2.Y) - y1;
            if (height > 0)
            {
                var edgeA = new Edge();
                var edgeB = new Edge();

                var stride = _colorRaster.Width;
                var offset = y1 * stride;

                var ax = s0.X - s2.X;
                var ay = s0.Y - s2.Y;
                var az = s0.Z - s2.Z;
                var bx = s1.X - s2.X;
                var by = s1.Y - s2.Y;
                var bz = s1.Z - s2.Z;

                var oneOverdX = 1.0f / (bx * ay - ax * by);
                var dZdX = oneOverdX * (bz * ay - az * by);
                var dZdY = oneOverdX * (az * bx - bz * ax);

                edgeA.Set(ref s0, ref s2, dZdX, dZdY);
                if ((height = edgeB.Set(ref s0, ref s1, dZdX, dZdY)) > 0)
                    if (swap)
                        offset = Rasterize(_zbPtr, _cbPtr, ref edgeA, ref edgeB, color, dZdX, offset, height, stride);
                    else
                        offset = Rasterize(_zbPtr, _cbPtr, ref edgeB, ref edgeA, color, dZdX, offset, height, stride);

                if ((height = edgeB.Set(ref s1, ref s2, dZdX, dZdY)) > 0)
                {
                    if (swap)
                        Rasterize(_zbPtr, _cbPtr, ref edgeA, ref edgeB, color, dZdX, offset, height, stride);
                    else
                        Rasterize(_zbPtr, _cbPtr, ref edgeB, ref edgeA, color, dZdX, offset, height, stride);
                }
            }
        }

        private static unsafe int Rasterize(float* zbPtr, int* cbPtr, ref Edge edga, ref Edge edgb, int color,
            float dZdX, int offset, int height, int stride)
        {
            var edgaX = edga.X;
            var edgbX = edgb.X;
            var edgbZ = edgb.Z;
            var edgaXStep = edga.XStep;
            var edgbXStep = edgb.XStep;
            var edgbZStep = edgb.ZStep;
            do
            {
                var start = Function.Ceil(edgbX);
                var width = Function.Ceil(edgaX) - start;
                if (width > 0)
                {
                    var mem = offset + start;
                    var zbmem = zbPtr + mem;
                    var cbmem = cbPtr + mem;
                    var w = edgbZ + (start - edgbX) * dZdX;
                    do
                    {
                        if (w < *zbmem)
                        {
                            *zbmem = w;
                            *cbmem = color;
                        }

                        w += dZdX;
                        ++zbmem;
                        ++cbmem;
                    } while (--width > 0);
                }

                edgaX += edgaXStep;
                edgbX += edgbXStep;
                edgbZ += edgbZStep;
                offset += stride;
            } while (--height > 0);

            edga.X = edgaX;
            edgb.X = edgbX;
            edgb.Z = edgbZ;
            return offset;
        }

        private struct Mats
        {
            public readonly int KdR;
            public readonly int KdG;
            public readonly int KdB;
            public readonly int KaR;
            public readonly int KaG;
            public readonly int KaB;

            public Mats(Material material)
            {
                KdR = (int) (material.Diffuse.Red * 0xFF);
                KdG = (int) (material.Diffuse.Grn * 0xFF);
                KdB = (int) (material.Diffuse.Blu * 0xFF);
                KaR = (int) (material.Ambient.Red * 0xFF);
                KaG = (int) (material.Ambient.Grn * 0xFF);
                KaB = (int) (material.Ambient.Blu * 0xFF);
            }
        }

        private struct Edge
        {
            public float X;
            public float XStep;
            public float Z;
            public float ZStep;

            public int Set(ref Vector3F va, ref Vector3F vb, float dZdX, float dZdY)
            {
                var y = Function.Ceil(va.Y);

                XStep = (vb.X - va.X) / (vb.Y - va.Y);
                ZStep = XStep * dZdX + dZdY;

                Z = y - va.Y;
                X = XStep * Z + va.X;
                Z = Z * dZdY + (X - va.X) * dZdX + va.Z;

                return Function.Ceil(vb.Y) - y;
            }
        }
    }
}