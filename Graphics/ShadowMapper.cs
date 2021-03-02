using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace IrregularZ.Graphics
{
    public sealed class ShadowMapper : IRenderer
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly Camera _vantage = new Camera((float) Math.PI / 4F, 1, 1, 1000);

        private readonly int _size;
        private readonly Grid _grid;
        private readonly Matrix4 _viewportMatrix;
        private byte[] _shadowMask = Array.Empty<byte>();

        public ShadowMapper(int size)
        {
            _size = size;
            _grid = new Grid(size);
            _viewportMatrix = Matrix4.CreateViewportMatrix(size, size);
        }

        public bool Clipping { get; set; }

        public Clipper Clipper { get; set; }

        public Color Material { get; set; }

        public Matrix4 WorldMatrix { get; set; }

        public void MoveTo(float x, float y, float z) => _vantage.MoveTo(x, y, z);

        public void LookAt(float x, float y, float z) => _vantage.LookAt(x, y, z);

        public void Generate(Matrix4 toWorldMatrix, FrameBuffer<float> depthBuffer)
        {
            if (_shadowMask.Length < depthBuffer.Size) _shadowMask = new byte[depthBuffer.Size];

            _watch.Restart();
            SpatialAcceleration(depthBuffer, toWorldMatrix);
            // Console.WriteLine("Spatial acceleration took {0}", _watch.ElapsedMilliseconds.ToString());
        }

        public void Render(Scene scene, FrameBuffer<int> colorBuffer)
        {
            _watch.Restart();
            scene.Render(this, new Frustum(_vantage.ViewMatrix, _vantage.ProjectionMatrix));
            // Console.WriteLine("Rendering took {0}", _watch.ElapsedMilliseconds.ToString());

            _watch.Restart();
            PostProcess(_shadowMask, colorBuffer);
            // Console.WriteLine("Post processing took {0}", _watch.ElapsedMilliseconds.ToString());
        }

        private void SpatialAcceleration(FrameBuffer<float> depthBuffer, Matrix4 matrix)
        {
            _grid.Reset();
            var offset = -1;
            var proj = _viewportMatrix * _vantage.ProjectionMatrix * _vantage.ViewMatrix * Matrix4.Invert(matrix);
            for (var y = 0; y < depthBuffer.Height; ++y)
            {
                for (var x = 0; x < depthBuffer.Width; ++x)
                {
                    var z = depthBuffer.Data[++offset];
                    if (z >= 1F) continue;
                    _grid.Set(Vector3.Project(proj, new Vector3(x, y, z)), offset);
                }
            }
        }

        public void Render(float[] vertexBuffer, int[] indexBuffer)
        {
            var matrix = _viewportMatrix * _vantage.ProjectionMatrix * _vantage.ViewMatrix * WorldMatrix;
            Parallel.ForEach(Partitioner.Create(0, indexBuffer.Length / 3), range =>
            {
                var (start, end) = range;
                for (var i = start * 3; i < end * 3; i += 3)
                {
                    var i0 = indexBuffer[i + 0] * 3;
                    var v0 = Vector3.Project(matrix, new Vector3(vertexBuffer[i0], vertexBuffer[i0 + 1], vertexBuffer[i0 + 2]));

                    var i1 = indexBuffer[i + 1] * 3;
                    var v1 = Vector3.Project(matrix, new Vector3(vertexBuffer[i1], vertexBuffer[i1 + 1], vertexBuffer[i1 + 2]));

                    var i2 = indexBuffer[i + 2] * 3;
                    var v2 = Vector3.Project(matrix, new Vector3(vertexBuffer[i2], vertexBuffer[i2 + 1], vertexBuffer[i2 + 2]));

                    var v10X = v1.X - v0.X;
                    var v21X = v2.X - v1.X;
                    var v10Y = v1.Y - v0.Y;
                    var v21Y = v2.Y - v1.Y;
                    if (v10Y * v21X - v10X * v21Y > 0F) continue;
                    
                    var v02Y = v0.Y - v2.Y;
                    var v02X = v0.X - v2.X;
                    var p0 = v21Y * v1.X - v21X * v1.Y;
                    var p1 = v02Y * v2.X - v02X * v2.Y;
                    var p2 = v10Y * v0.X - v10X * v0.Y;
                    var c1 = v1.Z - v0.Z;
                    var c2 = v2.Z - v0.Z;
                    var c3 = v02X * v10Y - v10X * v02Y;

                    var xMin = (int) MathF.Floor(MathF.Max(Min(v0.X, v1.X, v2.X), 0));
                    var yMin = (int) MathF.Floor(MathF.Max(Min(v0.Y, v1.Y, v2.Y), 0));
                    var xMax = (int) MathF.Ceiling(MathF.Min(Max(v0.X, v1.X, v2.X), _size - 1));
                    var yMax = (int) MathF.Ceiling(MathF.Min(Max(v0.Y, v1.Y, v2.Y), _size - 1));
                    for (var y = yMin; y < yMax; ++y)
                    {
                        for (var x = xMin; x < xMax; ++x)
                        {
                            var cell = _grid.Blocks[y * _size + x].Root;
                            while ((cell = cell.Next) != null)
                            {
                                if (p0 - v21Y * cell.X + v21X * cell.Y < 0F) continue;

                                var e1 = p1 - v02Y * cell.X + v02X * cell.Y;
                                if (e1 < 0F) continue;

                                var e2 = p2 - v10Y * cell.X + v10X * cell.Y;
                                if (e2 < 0F) continue;

                                if ((e1 * c1 + e2 * c2) / c3 < cell.Z - v0.Z - 0.000025F)
                                {
                                    _shadowMask[cell.Offset] |= 1;
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
            private Cell[] _cells;
            private int _index;

            public readonly LinkedList[] Blocks;

            public Grid(int size)
            {
                _size = size;
                _cells = new Cell[8];
                for (var i = 0; i < _cells.Length; i++) _cells[i] = new Cell();

                Blocks = new LinkedList[size * size];
                for (var i = 0; i < Blocks.Length; i++) Blocks[i] = new LinkedList();
            }

            public void Set(in Vector3 v, int offset)
            {
                var mx = (int) MathF.Floor(v.X); if (mx < 0 || mx >= _size) return;
                var my = (int) MathF.Floor(v.Y); if (my < 0 || my >= _size) return;
                EnsureCapacity();
                var cell = _cells[_index++];
                cell.X = v.X;
                cell.Y = v.Y;
                cell.Z = v.Z;
                cell.Offset = offset;
                cell.Next = null;
                Blocks[my * _size + mx].Push(cell);
            }

            public void Reset()
            {
                _index = 0;
                foreach (var blocks in Blocks) blocks.Reset();
            }

            private void EnsureCapacity()
            {
                if (_index < _cells.Length) return;
                Array.Resize(ref _cells, _cells.Length << 1);
                for (var i = _cells.Length >> 1; i < _cells.Length; ++i) _cells[i] = new Cell();
            }
        }

        private class LinkedList
        {
            public readonly Cell Root = new Cell();
            private Cell _current;

            public void Push(Cell cell) => _current = _current.Next = cell;

            public void Reset()
            {
                _current = Root;
                _current.Next = null;
            }
        }

        private class Cell
        {
            public float X;
            public float Y;
            public float Z;
            public int Offset;
            public Cell Next;
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