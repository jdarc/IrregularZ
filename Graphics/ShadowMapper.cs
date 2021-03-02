using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace IrregularZ.Graphics
{
    public sealed unsafe class ShadowMapper : IRenderer
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly Grid _grid;
        private readonly int _size;
        private readonly Camera _vantage;
        private byte[] _shadowMask = new byte[4];
        private Vector3[] _transformed = new Vector3[16];
        private readonly Matrix4 _viewportMatrix;
        private Matrix4 _worldMatrix;

        public ShadowMapper(int size)
        {
            _vantage = new Camera((float) Math.PI / 4F, 1, 1, 1, 1000);
            _size = size;
            _viewportMatrix = Matrix4.CreateViewportMatrix(size, size);
            _grid = new Grid(size);
        }

        public Clipper Clipper { get; set; }

        public Color Material { get; set; }

        public Matrix4 WorldMatrix
        {
            get => _worldMatrix;
            set => _worldMatrix = value;
        }

        public void MoveTo(float x, float y, float z) => _vantage.MoveTo(x, y, z);

        public void LookAt(float x, float y, float z) => _vantage.LookAt(x, y, z);

        public void Generate(FrameBuffer<float> depthBuffer, Matrix4 toWorldMatrix)
        {
            if (_shadowMask.Length < depthBuffer.Size) _shadowMask = new byte[depthBuffer.Size];

            _watch.Restart();
            SpatialAcceleration(depthBuffer, toWorldMatrix);
            Console.WriteLine("Spatial acceleration took {0}", _watch.ElapsedMilliseconds.ToString());
        }

        public void Render(Scene scene, FrameBuffer<int> colorBuffer)
        {
            _watch.Restart();
            scene.Render(this, new Frustum(_vantage.ViewMatrix, _vantage.ProjectionMatrix));
            Console.WriteLine("Rendering took {0}", _watch.ElapsedMilliseconds.ToString());

            _watch.Restart();
            PostProcess(_shadowMask, colorBuffer);
            Console.WriteLine("Post processing took {0}", _watch.ElapsedMilliseconds.ToString());

            Console.WriteLine();
        }

        private void SpatialAcceleration(FrameBuffer<float> depthBuffer, Matrix4 matrix)
        {
            _grid.Reset(depthBuffer.Size);
            var proj = _viewportMatrix * _vantage.ProjectionMatrix * _vantage.ViewMatrix * Matrix4.Invert(matrix);
            for (var y = 0; y < depthBuffer.Height; ++y)
            {
                for (var x = 0; x < depthBuffer.Width; ++x)
                {
                    var z = depthBuffer.Data[y * depthBuffer.Width + x];
                    if (z >= 1F) continue;
                    var sx = proj.M11 * x + proj.M12 * y + proj.M13 * z + proj.M14;
                    var sy = proj.M21 * x + proj.M22 * y + proj.M23 * z + proj.M24;
                    var sz = proj.M31 * x + proj.M32 * y + proj.M33 * z + proj.M34;
                    var sw = proj.M41 * x + proj.M42 * y + proj.M43 * z + proj.M44;
                    _grid.Set(sx / sw, sy / sw, sz / sw, y * depthBuffer.Width + x);
                }
            }
        }

        public void Render(float[] vertexBuffer, int[] indexBuffer)
        {
            var matrix = _viewportMatrix * _vantage.ProjectionMatrix * _vantage.ViewMatrix * _worldMatrix;

            var triangleCount = indexBuffer.Length / 3;
            if (_transformed.Length < vertexBuffer.Length) _transformed = new Vector3[vertexBuffer.Length];

            Parallel.ForEach(Partitioner.Create(0, vertexBuffer.Length / 3), range =>
            {
                fixed (Vector3* dst = _transformed)
                {
                    var (start, end) = range;
                    for (var i = start; i < end; ++i)
                    {
                        var vx = vertexBuffer[i * 3 + 0];
                        var vy = vertexBuffer[i * 3 + 1];
                        var vz = vertexBuffer[i * 3 + 2];
                        var x = matrix.M11 * vx + matrix.M12 * vy + matrix.M13 * vz + matrix.M14;
                        var y = matrix.M21 * vx + matrix.M22 * vy + matrix.M23 * vz + matrix.M24;
                        var z = matrix.M31 * vx + matrix.M32 * vy + matrix.M33 * vz + matrix.M34;
                        var w = matrix.M41 * vx + matrix.M42 * vy + matrix.M43 * vz + matrix.M44;
                        *(dst + i) = new Vector3(x / w, y / w, z / w);
                    }
                }
            });

            Parallel.ForEach(Partitioner.Create(0, triangleCount), range =>
            {
                fixed (LinkedList* blocks = _grid.Blocks)
                {
                    fixed (int* ix = indexBuffer)
                    {
                        fixed (Vector3* dst = _transformed)
                        {
                            var (start, end) = range;
                            for (var i = start; i < end; i++)
                            {
                                var v0 = dst + (ix + i * 3)[0];
                                var v1 = dst + (ix + i * 3)[1];
                                var v2 = dst + (ix + i * 3)[2];

                                var v10X = v1->X - v0->X;
                                var v21X = v2->X - v1->X;
                                var v10Y = v1->Y - v0->Y;
                                var v21Y = v2->Y - v1->Y;
                                if (v10Y * v21X - v10X * v21Y >= 0F) continue;

                                var minx = (int) MathF.Floor(MathF.Max(Min(v0->X, v1->X, v2->X), 0));
                                var miny = (int) MathF.Floor(MathF.Max(Min(v0->Y, v1->Y, v2->Y), 0));
                                var maxx = (int) MathF.Ceiling(MathF.Min(Max(v0->X, v1->X, v2->X), _size - 1));
                                var maxy = (int) MathF.Ceiling(MathF.Min(Max(v0->Y, v1->Y, v2->Y), _size - 1));

                                var v02Y = v0->Y - v2->Y;
                                var v02X = v0->X - v2->X;
                                var p0 = v21Y * v1->X - v21X * v1->Y;
                                var p1 = v02Y * v2->X - v02X * v2->Y;
                                var p2 = v10Y * v0->X - v10X * v0->Y;
                                var invarea = 1 / (v02X * v10Y - v10X * v02Y);
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
                                                var dist = v0->Z + invarea * (e1 * c1 + e2 * c2) + 0.00005F;
                                                if (dist < cell->Z)
                                                {
                                                    _shadowMask[cell->Offset] = 1;
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

        private sealed class Grid
        {
            private readonly int _size;
            public readonly LinkedList[] Blocks;
            private Cell* _rootCell;
            private Cell[] _cells;
            private GCHandle _gcHandle;
            private Cell* _nextCell;

            public Grid(int size)
            {
                _size = size;
                Blocks = new LinkedList[size * size];
            }

            ~Grid()
            {
                if (_gcHandle.IsAllocated) _gcHandle.Free();
            }

            public void Reset(int size)
            {
                if (_cells == null || _cells.Length < size + Blocks.Length)
                {
                    if (_gcHandle.IsAllocated) _gcHandle.Free();
                    _cells = new Cell[size + Blocks.Length];
                    _gcHandle = GCHandle.Alloc(_cells, GCHandleType.Pinned);
                    _rootCell = (Cell*) _gcHandle.AddrOfPinnedObject();
                }
                _nextCell = _rootCell;
                for (var i = 0; i < Blocks.Length; ++i) Blocks[i].Reset(_nextCell++);
            }

            public void Set(float x, float y, float z, int offset)
            {
                var mx = (int) MathF.Floor(x);
                var my = (int) MathF.Floor(y);
                if (mx < 0 || my < 0 || mx >= _size || my >= _size) return;
                var cell = _nextCell++;
                cell->X = x;
                cell->Y = y;
                cell->Z = z;
                cell->Offset = offset;
                cell->Next = null;
                Blocks[my * _size + mx].Add(cell);
            }
        }

        private struct LinkedList
        {
            public Cell* Root;
            private Cell* _current;

            public void Reset(Cell* root)
            {
                Root = _current = root;
                Root->Next = null;
            }

            public void Add(Cell* cell) => _current = _current->Next = cell;
        }

        private struct Cell
        {
            public float X;
            public float Y;
            public float Z;
            public int Offset;
            public Cell* Next;
        }

        private static void PostProcess(IList<byte> shadowBits, FrameBuffer<int> colorBuffer)
        {
            Parallel.ForEach(Partitioner.Create(0, colorBuffer.Height), range =>
            {
                var (start, end) = range;
                for (var y = start; y < end; ++y)
                {
                    var offset = y * colorBuffer.Width;
                    for (var x = 0; x < colorBuffer.Width; ++x)
                    {
                        if (shadowBits[offset + x] == 0) continue;
                        colorBuffer.Data[offset + x] = 0xFF << 0x18 | (colorBuffer.Data[offset + x] >> 1) & 0x7F7F7F;
                        shadowBits[offset + x] = 0;
                    }
                }
            });
        }

        private static float Min(float a, float b, float c) => MathF.Min(a, MathF.Min(b, c));

        private static float Max(float a, float b, float c) => MathF.Max(a, MathF.Max(b, c));
    }
}