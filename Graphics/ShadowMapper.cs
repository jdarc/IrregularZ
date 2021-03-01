using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IrregularZ.Graphics
{
    public sealed unsafe class ShadowMapper : IRenderer
    {
        private readonly Grid _grid;
        private readonly int _size;
        private readonly Camera _vantage;
        private Matrix4x4 _combMatrix;
        private byte[] _shadowBits = new byte[4];
        private Tuple3[] _transformed = new Tuple3[16];
        private readonly Matrix4x4 _viewportMatrix;
        private Matrix4x4 _worldMatrix;

        public ShadowMapper(int size)
        {
            _vantage = new Camera((float) Math.PI / 4.0f, 1, 1, 1f, 10000f);
            _size = size;
            _viewportMatrix = Matrix4x4.CreateViewportMatrix(size, size);
            _grid = new Grid(size);
            Clipping = true;
        }

        public Frustum Frustum { get; set; }
        public bool Clipping { get; set; }
        public Color Material { get; set; }

        public void Dispose()
        {
        }

        public Matrix4x4 WorldMatrix
        {
            get => _worldMatrix;
            set => _worldMatrix = value;
        }

        public Matrix4x4 ViewMatrix
        {
            get => _vantage.ViewMatrix;
            set { }
        }

        public Matrix4x4 ProjectionMatrix
        {
            get => _vantage.ProjectionMatrix;
            set { }
        }

        public void Render(float[] vertexBuffer, int[] indexBuffer)
        {
            var triangleCount = indexBuffer.Length / 3;
            if (_transformed.Length < vertexBuffer.Length) _transformed = new Tuple3[vertexBuffer.Length];

            var e11 = _combMatrix.M11 * _worldMatrix.M11 + _combMatrix.M12 * _worldMatrix.M21 +
                      _combMatrix.M13 * _worldMatrix.M31 +
                      _combMatrix.M14 * _worldMatrix.M41;
            var e12 = _combMatrix.M11 * _worldMatrix.M12 + _combMatrix.M12 * _worldMatrix.M22 +
                      _combMatrix.M13 * _worldMatrix.M32 +
                      _combMatrix.M14 * _worldMatrix.M42;
            var e13 = _combMatrix.M11 * _worldMatrix.M13 + _combMatrix.M12 * _worldMatrix.M23 +
                      _combMatrix.M13 * _worldMatrix.M33 +
                      _combMatrix.M14 * _worldMatrix.M43;
            var e14 = _combMatrix.M11 * _worldMatrix.M14 + _combMatrix.M12 * _worldMatrix.M24 +
                      _combMatrix.M13 * _worldMatrix.M34 +
                      _combMatrix.M14 * _worldMatrix.M44;
            var e21 = _combMatrix.M21 * _worldMatrix.M11 + _combMatrix.M22 * _worldMatrix.M21 +
                      _combMatrix.M23 * _worldMatrix.M31 +
                      _combMatrix.M24 * _worldMatrix.M41;
            var e22 = _combMatrix.M21 * _worldMatrix.M12 + _combMatrix.M22 * _worldMatrix.M22 +
                      _combMatrix.M23 * _worldMatrix.M32 +
                      _combMatrix.M24 * _worldMatrix.M42;
            var e23 = _combMatrix.M21 * _worldMatrix.M13 + _combMatrix.M22 * _worldMatrix.M23 +
                      _combMatrix.M23 * _worldMatrix.M33 +
                      _combMatrix.M24 * _worldMatrix.M43;
            var e24 = _combMatrix.M21 * _worldMatrix.M14 + _combMatrix.M22 * _worldMatrix.M24 +
                      _combMatrix.M23 * _worldMatrix.M34 +
                      _combMatrix.M24 * _worldMatrix.M44;
            var e31 = _combMatrix.M31 * _worldMatrix.M11 + _combMatrix.M32 * _worldMatrix.M21 +
                      _combMatrix.M33 * _worldMatrix.M31 +
                      _combMatrix.M34 * _worldMatrix.M41;
            var e32 = _combMatrix.M31 * _worldMatrix.M12 + _combMatrix.M32 * _worldMatrix.M22 +
                      _combMatrix.M33 * _worldMatrix.M32 +
                      _combMatrix.M34 * _worldMatrix.M42;
            var e33 = _combMatrix.M31 * _worldMatrix.M13 + _combMatrix.M32 * _worldMatrix.M23 +
                      _combMatrix.M33 * _worldMatrix.M33 +
                      _combMatrix.M34 * _worldMatrix.M43;
            var e34 = _combMatrix.M31 * _worldMatrix.M14 + _combMatrix.M32 * _worldMatrix.M24 +
                      _combMatrix.M33 * _worldMatrix.M34 +
                      _combMatrix.M34 * _worldMatrix.M44;
            var e41 = _combMatrix.M41 * _worldMatrix.M11 + _combMatrix.M42 * _worldMatrix.M21 +
                      _combMatrix.M43 * _worldMatrix.M31 +
                      _combMatrix.M44 * _worldMatrix.M41;
            var e42 = _combMatrix.M41 * _worldMatrix.M12 + _combMatrix.M42 * _worldMatrix.M22 +
                      _combMatrix.M43 * _worldMatrix.M32 +
                      _combMatrix.M44 * _worldMatrix.M42;
            var e43 = _combMatrix.M41 * _worldMatrix.M13 + _combMatrix.M42 * _worldMatrix.M23 +
                      _combMatrix.M43 * _worldMatrix.M33 +
                      _combMatrix.M44 * _worldMatrix.M43;
            var e44 = _combMatrix.M41 * _worldMatrix.M14 + _combMatrix.M42 * _worldMatrix.M24 +
                      _combMatrix.M43 * _worldMatrix.M34 +
                      _combMatrix.M44 * _worldMatrix.M44;

            Parallel.ForEach(Partitioner.Create(0, vertexBuffer.Length / 3), (range, state) =>
            {
                fixed (Tuple3* dst = _transformed)
                {
                    for (var i = range.Item1; i < range.Item2; ++i)
                    {
                        var sX = vertexBuffer[i * 3];
                        var sY = vertexBuffer[i * 3 + 1];
                        var sZ = vertexBuffer[i * 3 + 2];
                        var t = dst + i;
                        var w = 1F / (e41 * sX + e42 * sY + e43 * sZ + e44);
                        t->X = (e11 * sX + e12 * sY + e13 * sZ + e14) * w;
                        t->Y = (e21 * sX + e22 * sY + e23 * sZ + e24) * w;
                        t->Z = e31 * sX + e32 * sY + e33 * sZ + e34;
                    }
                }
            });

            Parallel.ForEach(Partitioner.Create(0, triangleCount), (range, state) =>
            {
                fixed (Block* blocks = _grid.Blocks)
                {
                    fixed (int* ix = indexBuffer)
                    {
                        fixed (Tuple3* dst = _transformed)
                        {
                            for (var i = range.Item1; i < range.Item2; i++)
                            {
                                var lix = ix + i * 3;
                                var v0 = dst + lix[0];
                                var v1 = dst + lix[1];
                                var v2 = dst + lix[2];

                                var v10X = v1->X - v0->X;
                                var v21X = v2->X - v1->X;
                                var v10Y = v1->Y - v0->Y;
                                var v21Y = v2->Y - v1->Y;
                                if (!(v10Y * v21X - v10X * v21Y < 0)) continue;

                                // Do actual rasterization of scanlines
                                var b2 = Min(v0->X, v1->X, v2->X);
                                var minx = (int) MathF.Max(0, b2);
                                var b = Max(v0->X, v1->X, v2->X) + 1;
                                var maxx = (int) MathF.Min(_size - 1, b);
                                var b3 = Min(v0->Y, v1->Y, v2->Y);
                                var miny = (int) MathF.Max(0, b3);
                                var b1 = Max(v0->Y, v1->Y, v2->Y) + 1;
                                var maxy = (int) MathF.Min(_size - 1, b1);

                                var v02Y = v0->Y - v2->Y;
                                var v02X = v0->X - v2->X;
                                var p0 = v21Y * v1->X - v21X * v1->Y;
                                var p1 = v02Y * v2->X - v02X * v2->Y;
                                var p2 = v10Y * v0->X - v10X * v0->Y;
                                var invarea = 1.0f / (v02X * v10Y - v10X * v02Y);
                                var c1 = v1->Z - v0->Z;
                                var c2 = v2->Z - v0->Z;

                                for (var y = miny; y < maxy; ++y)
                                {
                                    var bmem = blocks + y * _size + minx;
                                    for (var x = minx; x < maxx; ++x)
                                    {
                                        var block = bmem++;
                                        var prev = block->Root;
                                        var cell = prev;
                                        while ((cell = cell->Next) != null)
                                        {
                                            var mx = cell->X;
                                            var my = cell->Y;
                                            var e0 = p0 + v21X * my - v21Y * mx;
                                            var e1 = p1 + v02X * my - v02Y * mx;
                                            var e2 = p2 + v10X * my - v10Y * mx;
                                            if (e0 >= 0 && e1 >= 0 && e2 >= 0)
                                            {
                                                var dist = v0->Z + invarea * (e1 * c1 + e2 * c2) + 0.0005f;
                                                if (dist < cell->Z)
                                                {
                                                    _shadowBits[cell->Mem] = 1;
                                                    prev->Next = cell->Next;
                                                }
                                                else
                                                {
                                                    prev = cell;
                                                }
                                            }
                                            else
                                            {
                                                prev = cell;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public void MoveTo(float x, float y, float z)
        {
            _vantage.MoveTo(x, y, z);
        }

        public void LookAt(float x, float y, float z)
        {
            _vantage.LookAt(x, y, z);
        }

        public void Shadow(Scene.Scene scene, FrameBuffer<int> colorBuffer, FrameBuffer<float> depthBuffer,
            Matrix4x4 combMatrix)
        {
            var watch = new Stopwatch();

            watch.Reset();
            watch.Start();
            if (_shadowBits.Length < colorBuffer.Width * colorBuffer.Height)
                _shadowBits = new byte[colorBuffer.Width * colorBuffer.Height];
            _grid.Reset(colorBuffer.Size);
            watch.Stop();

            watch.Reset();
            watch.Start();
            SpatialAcceleration(depthBuffer, combMatrix);
            watch.Stop();
            // Console.WriteLine("Spatial acceleration took {0}", watch.ElapsedMilliseconds.ToString());

            watch.Reset();
            watch.Start();
            scene.Render(this, new Frustum(_vantage.ViewMatrix, _vantage.ProjectionMatrix));
            watch.Stop();
            // Console.WriteLine("Render took {0}", watch.ElapsedMilliseconds.ToString());

            watch.Reset();
            watch.Start();
            PostProcess(colorBuffer);
            watch.Stop();
            // Console.WriteLine("Post process took {0}", watch.ElapsedMilliseconds.ToString());
            // Console.WriteLine();
        }

        private void PostProcess(FrameBuffer<int> colorBuffer)
        {
            var width = colorBuffer.Width;
            var height = colorBuffer.Height;
            fixed (int* cptr = colorBuffer.Data)
                for (var y = 0; y < height; y++)
                {
                    var bmem = y * width;
                    for (var x = 0; x < width; ++x)
                    {
                        if (_shadowBits[bmem] > 0)
                        {
                            _shadowBits[bmem] = 0;
                            cptr[bmem] = (0xFF << 0x18) | ((cptr[bmem] >> 1) & 0x7f7f7f);
                        }

                        ++bmem;
                    }
                }
        }

        private void SpatialAcceleration(FrameBuffer<float> depthBuffer, Matrix4x4 mcombMatrix)
        {
            _combMatrix = _viewportMatrix * _vantage.ProjectionMatrix * _vantage.ViewMatrix;
            mcombMatrix = Matrix4x4.Invert(mcombMatrix);
            var mem = 0;
            fixed (float* zbPtr = depthBuffer.Data)
            {
                for (var y = 0; y < depthBuffer.Height; ++y)
                for (var x = 0; x < depthBuffer.Width; ++x)
                {
                    var z = zbPtr[mem];
                    if (z < 1)
                    {
                        var sx = mcombMatrix.M11 * x + mcombMatrix.M12 * y + mcombMatrix.M13 * z + mcombMatrix.M14;
                        var sy = mcombMatrix.M21 * x + mcombMatrix.M22 * y + mcombMatrix.M23 * z + mcombMatrix.M24;
                        var sz = mcombMatrix.M31 * x + mcombMatrix.M32 * y + mcombMatrix.M33 * z + mcombMatrix.M34;
                        var sw = mcombMatrix.M41 * x + mcombMatrix.M42 * y + mcombMatrix.M43 * z + mcombMatrix.M44;
                        sx /= sw;
                        sy /= sw;
                        sz /= sw;

                        var tw = _combMatrix.M41 * sx + _combMatrix.M42 * sy + _combMatrix.M43 * sz + _combMatrix.M44;
                        var tx = _combMatrix.M11 * sx + _combMatrix.M12 * sy + _combMatrix.M13 * sz + _combMatrix.M14;
                        var ty = _combMatrix.M21 * sx + _combMatrix.M22 * sy + _combMatrix.M23 * sz + _combMatrix.M24;
                        var tz = _combMatrix.M31 * sx + _combMatrix.M32 * sy + _combMatrix.M33 * sz + _combMatrix.M34;
                        _grid.Set(tx / tw, ty / tw, tz, mem);
                    }

                    ++mem;
                }
            }
        }

        private static float Min(float a, float b, float c)
        {
            return MathF.Min(a, MathF.Min(b, c));
        }

        private static float Max(float a, float b, float c)
        {
            return MathF.Max(a, MathF.Max(b, c));
        }

        private sealed class Grid
        {
            private readonly int _size;
            public readonly Block[] Blocks;
            private Cell* _cell;
            private Cell[] _cells;
            private GCHandle _gcHandle;
            private Cell* _nextCell;

            public Grid(int size)
            {
                _size = size;
                Blocks = new Block[size * size];
            }

            ~Grid()
            {
                if (_gcHandle.IsAllocated) _gcHandle.Free();
            }

            public void Reset(int size)
            {
                EnsureCapacity(size);
                for (var i = 0; i < Blocks.Length; ++i) Blocks[i].Reset(_nextCell++);
            }

            private void EnsureCapacity(int size)
            {
                if (_cells == null || _cells.Length < size + Blocks.Length)
                {
                    if (_gcHandle.IsAllocated) _gcHandle.Free();
                    _cells = new Cell[size + Blocks.Length];
                    _gcHandle = GCHandle.Alloc(_cells, GCHandleType.Pinned);
                    _cell = (Cell*) _gcHandle.AddrOfPinnedObject();
                }

                _nextCell = _cell;
            }

            public void Set(float x, float y, float z, int mem)
            {
                var mx = (int) x;
                var my = (int) y;
                if (mx >= 0 && my >= 0 && mx < _size && my < _size)
                {
                    var cell = _nextCell++;
                    cell->X = x;
                    cell->Y = y;
                    cell->Z = z;
                    cell->Mem = mem;
                    cell->Next = null;
                    Blocks[my * _size + mx].Add(cell);
                }
            }
        }

        private struct Block
        {
            public Cell* Root;
            private Cell* _current;

            public void Reset(Cell* root)
            {
                Root = _current = root;
                Root->Next = null;
            }

            public void Add(Cell* cell)
            {
                _current = _current->Next = cell;
            }
        }

        private struct Cell
        {
            public float X;
            public float Y;
            public float Z;
            public int Mem;
            public Cell* Next;
        }
    }
}