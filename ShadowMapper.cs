using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IrregularZ
{
    public sealed unsafe class ShadowMapper : IVisualizer
    {
        private readonly Grid _grid;
        private readonly int _size;
        private readonly Camera _vantage;
        private Matrix4F _combMatrix;
        private byte[] _shadowBits = new byte[4];
        private Vector3F[] _transformed = new Vector3F[16];
        private Matrix4F _viewportMatrix;
        private Matrix4F _worldMatrix;

        public ShadowMapper(int size)
        {
            _vantage = new Camera((float) Math.PI / 4.0f, 1, 1, 1f, 10000f);
            _size = size;
            var hsze = size * 0.5f;
            _viewportMatrix = new Matrix4F(hsze, 0, 0, hsze - 0.5f, 0, hsze, 0, hsze - 0.5f, 0, 0, 1, 0, 0, 0, 0, 1);
            _grid = new Grid(size);
            Clipping = true;
        }

        public Frustum Frustum { get; set; }
        public bool Clipping { get; set; }

        public void Dispose()
        {
        }

        public Matrix4F WorldMatrix
        {
            get => _worldMatrix;
            set => _worldMatrix = value;
        }

        public Matrix4F ViewMatrix
        {
            get => _vantage.ViewMatrix;
            set { }
        }

        public Matrix4F ProjectionMatrix
        {
            get => _vantage.ProjectionMatrix;
            set { }
        }

        public Matrix4F CombinedMatrix => _combMatrix;

        public void Draw(Mesh mesh, Material material)
        {
            Draw(mesh.Vertices, mesh.Indices);
        }

        public void Clear(int rgb, float depth)
        {
        }

        public void Draw(Vector3F[] vertexBuffer, int[] indexBuffer)
        {
            var triangleCount = indexBuffer.Length / 3;
            if (_transformed.Length < vertexBuffer.Length) _transformed = new Vector3F[vertexBuffer.Length];

            var e11 = _combMatrix.E11 * _worldMatrix.E11 + _combMatrix.E12 * _worldMatrix.E21 +
                      _combMatrix.E13 * _worldMatrix.E31 + _combMatrix.E14 * _worldMatrix.E41;
            var e12 = _combMatrix.E11 * _worldMatrix.E12 + _combMatrix.E12 * _worldMatrix.E22 +
                      _combMatrix.E13 * _worldMatrix.E32 + _combMatrix.E14 * _worldMatrix.E42;
            var e13 = _combMatrix.E11 * _worldMatrix.E13 + _combMatrix.E12 * _worldMatrix.E23 +
                      _combMatrix.E13 * _worldMatrix.E33 + _combMatrix.E14 * _worldMatrix.E43;
            var e14 = _combMatrix.E11 * _worldMatrix.E14 + _combMatrix.E12 * _worldMatrix.E24 +
                      _combMatrix.E13 * _worldMatrix.E34 + _combMatrix.E14 * _worldMatrix.E44;
            var e21 = _combMatrix.E21 * _worldMatrix.E11 + _combMatrix.E22 * _worldMatrix.E21 +
                      _combMatrix.E23 * _worldMatrix.E31 + _combMatrix.E24 * _worldMatrix.E41;
            var e22 = _combMatrix.E21 * _worldMatrix.E12 + _combMatrix.E22 * _worldMatrix.E22 +
                      _combMatrix.E23 * _worldMatrix.E32 + _combMatrix.E24 * _worldMatrix.E42;
            var e23 = _combMatrix.E21 * _worldMatrix.E13 + _combMatrix.E22 * _worldMatrix.E23 +
                      _combMatrix.E23 * _worldMatrix.E33 + _combMatrix.E24 * _worldMatrix.E43;
            var e24 = _combMatrix.E21 * _worldMatrix.E14 + _combMatrix.E22 * _worldMatrix.E24 +
                      _combMatrix.E23 * _worldMatrix.E34 + _combMatrix.E24 * _worldMatrix.E44;
            var e31 = _combMatrix.E31 * _worldMatrix.E11 + _combMatrix.E32 * _worldMatrix.E21 +
                      _combMatrix.E33 * _worldMatrix.E31 + _combMatrix.E34 * _worldMatrix.E41;
            var e32 = _combMatrix.E31 * _worldMatrix.E12 + _combMatrix.E32 * _worldMatrix.E22 +
                      _combMatrix.E33 * _worldMatrix.E32 + _combMatrix.E34 * _worldMatrix.E42;
            var e33 = _combMatrix.E31 * _worldMatrix.E13 + _combMatrix.E32 * _worldMatrix.E23 +
                      _combMatrix.E33 * _worldMatrix.E33 + _combMatrix.E34 * _worldMatrix.E43;
            var e34 = _combMatrix.E31 * _worldMatrix.E14 + _combMatrix.E32 * _worldMatrix.E24 +
                      _combMatrix.E33 * _worldMatrix.E34 + _combMatrix.E34 * _worldMatrix.E44;
            var e41 = _combMatrix.E41 * _worldMatrix.E11 + _combMatrix.E42 * _worldMatrix.E21 +
                      _combMatrix.E43 * _worldMatrix.E31 + _combMatrix.E44 * _worldMatrix.E41;
            var e42 = _combMatrix.E41 * _worldMatrix.E12 + _combMatrix.E42 * _worldMatrix.E22 +
                      _combMatrix.E43 * _worldMatrix.E32 + _combMatrix.E44 * _worldMatrix.E42;
            var e43 = _combMatrix.E41 * _worldMatrix.E13 + _combMatrix.E42 * _worldMatrix.E23 +
                      _combMatrix.E43 * _worldMatrix.E33 + _combMatrix.E44 * _worldMatrix.E43;
            var e44 = _combMatrix.E41 * _worldMatrix.E14 + _combMatrix.E42 * _worldMatrix.E24 +
                      _combMatrix.E43 * _worldMatrix.E34 + _combMatrix.E44 * _worldMatrix.E44;

            fixed (Vector3F* src = vertexBuffer, dst = _transformed)
            {
                for (var i = 0; i < vertexBuffer.Length; ++i)
                {
                    var s = src + i;
                    var t = dst + i;
                    var w = 1.0f / (e41 * s->X + e42 * s->Y + e43 * s->Z + e44);
                    t->X = (e11 * s->X + e12 * s->Y + e13 * s->Z + e14) * w;
                    t->Y = (e21 * s->X + e22 * s->Y + e23 * s->Z + e24) * w;
                    t->Z = e31 * s->X + e32 * s->Y + e33 * s->Z + e34;
                }
            }

            fixed (Block* blocks = _grid.Blocks)
            {
                fixed (int* ix = indexBuffer)
                {
                    fixed (Vector3F* dst = _transformed)
                    {
                        for (var i = 0; i < triangleCount; i++)
                        {
                            var lix = ix + i * 3;
                            var v0 = dst + lix[0];
                            var v1 = dst + lix[1];
                            var v2 = dst + lix[2];

                            var v10X = v1->X - v0->X;
                            var v21X = v2->X - v1->X;
                            var v10Y = v1->Y - v0->Y;
                            var v21Y = v2->Y - v1->Y;
                            if (v10Y * v21X - v10X * v21Y < 0)
                            {
                                // Do actual rasterization of scanlines
                                var minx = (int) Function.Max(0, Function.Min(v0->X, v1->X, v2->X));
                                var maxx = (int) Function.Min(_size - 1, Function.Max(v0->X, v1->X, v2->X) + 1);
                                var miny = (int) Function.Max(0, Function.Min(v0->Y, v1->Y, v2->Y));
                                var maxy = (int) Function.Min(_size - 1, Function.Max(v0->Y, v1->Y, v2->Y) + 1);

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
            }
        }

        public void MoveTo(float x, float y, float z)
        {
            _vantage.MoveTo(x, y, z);
        }

        public void LookAt(float x, float y, float z)
        {
            _vantage.LookAt(x, y, z);
        }

        public void Shadow(Scene scene, Raster colorBuffer, Raster depthBuffer, Matrix4F combMatrix)
        {
            var watch = new Stopwatch();

            watch.Reset();
            watch.Start();
            if (_shadowBits.Length < colorBuffer.Width * colorBuffer.Height)
                _shadowBits = new byte[colorBuffer.Width * colorBuffer.Height];
            _grid.Reset(colorBuffer.Buffer.Size);
            watch.Stop();

            watch.Reset();
            watch.Start();
            SpatialAcceleration(depthBuffer, combMatrix);
            watch.Stop();
            // Console.WriteLine("Spatial acceleration took {0}", watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            scene.Render(this, _vantage.Frustum);
            watch.Stop();
            // Console.WriteLine("Render took {0}", watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            PostProcess(colorBuffer);
            watch.Stop();
            // Console.WriteLine("Post process took {0}", watch.ElapsedMilliseconds);
            // Console.WriteLine();
        }

        private void PostProcess(Raster colorBuffer)
        {
            var width = colorBuffer.Width;
            var height = colorBuffer.Height;
            var cptr = (int*) colorBuffer.Buffer.Scan0;
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

        private void SpatialAcceleration(Raster depthBuffer, Matrix4F combMatrix)
        {
            var projectionMatrix = _vantage.ProjectionMatrix;
            var viewMatrix = _vantage.ViewMatrix;
            _combMatrix.Multiply(ref projectionMatrix, ref viewMatrix);
            _combMatrix.Multiply(ref _viewportMatrix, ref _combMatrix);

            combMatrix.Invert();
            var mem = 0;
            var height = depthBuffer.Height;
            var width = depthBuffer.Width;
            var zbPtr = (float*) depthBuffer.Buffer.Scan0;
            for (var y = 0; y < height; ++y)
            for (var x = 0; x < width; ++x)
            {
                var z = zbPtr[mem];
                if (z < 1)
                {
                    var sw = combMatrix.E41 * x + combMatrix.E42 * y + combMatrix.E43 * z + combMatrix.E44;
                    var sx = (combMatrix.E11 * x + combMatrix.E12 * y + combMatrix.E13 * z + combMatrix.E14) / sw;
                    var sy = (combMatrix.E21 * x + combMatrix.E22 * y + combMatrix.E23 * z + combMatrix.E24) / sw;
                    var sz = (combMatrix.E31 * x + combMatrix.E32 * y + combMatrix.E33 * z + combMatrix.E34) / sw;

                    var tw = _combMatrix.E41 * sx + _combMatrix.E42 * sy + _combMatrix.E43 * sz + _combMatrix.E44;
                    var tx = _combMatrix.E11 * sx + _combMatrix.E12 * sy + _combMatrix.E13 * sz + _combMatrix.E14;
                    var ty = _combMatrix.E21 * sx + _combMatrix.E22 * sy + _combMatrix.E23 * sz + _combMatrix.E24;
                    var tz = _combMatrix.E31 * sx + _combMatrix.E32 * sy + _combMatrix.E33 * sz + _combMatrix.E34;
                    _grid.Set(tx / tw, ty / tw, tz, mem);
                }

                ++mem;
            }
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