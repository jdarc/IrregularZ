using System.Collections.Generic;
using System.Drawing;

namespace IrregularZ.Graphics
{
    public class Assembler
    {
        private readonly List<Triangle> _triangles;
        private readonly List<Tuple3> _vertices;

        public Assembler()
        {
            _vertices = new List<Tuple3>();
            _triangles = new List<Triangle>();
        }

        public Color Color { get; set; } = Color.White;

        public void AddVertex(float x, float y, float z)
        {
            _vertices.Add(new Tuple3(x, y, z));
        }

        public void CreateTriangle(int a, int b, int c)
        {
            _triangles.Add(new Triangle(a, b, c) {Color = Color});
        }

        public Model Compile()
        {
            var matBuckets = new Dictionary<Color, List<Triangle>>();
            foreach (var triangle1 in _triangles)
            {
                if (!matBuckets.ContainsKey(triangle1.Color))
                    matBuckets.Add(triangle1.Color, new List<Triangle>());
                matBuckets[triangle1.Color].Add(triangle1);
            }

            var meshByMaterial = new Dictionary<Color, Mesh>();
            foreach (var (material, triangles) in matBuckets)
            {
                var elementCount = triangles.Count * 3;
                var ib = new int[elementCount];
                var vb1 = new Tuple3[elementCount];
                var idx = 0;
                foreach (var triangle2 in triangles)
                {
                    vb1[idx] = _vertices[triangle2.A];
                    ib[idx] = idx++;

                    vb1[idx] = _vertices[triangle2.B];
                    ib[idx] = idx++;

                    vb1[idx] = _vertices[triangle2.C];
                    ib[idx] = idx++;
                }

                meshByMaterial.Add(material, Optimize(vb1, ib));
            }

            return new Model(meshByMaterial);
        }

        private static Mesh Optimize(IReadOnlyList<Tuple3> vertices, IReadOnlyList<int> indices)
        {
            var vertexMap = new Dictionary<Tuple3, int>();
            var optTriangles = new int[indices.Count];
            for (var i = 0; i < indices.Count; i++)
            {
                var va = vertices[indices[i]];
                if (!vertexMap.ContainsKey(va)) vertexMap.Add(va, vertexMap.Count);
                optTriangles[i] = vertexMap[va];
            }

            var optVertices = new Tuple3[vertexMap.Count];
            vertexMap.Keys.CopyTo(optVertices, 0);

            var vertexBuffer = new List<float>();
            foreach (var vertex in optVertices)
            {
                vertexBuffer.Add(vertex.X);
                vertexBuffer.Add(vertex.Y);
                vertexBuffer.Add(vertex.Z);
            }

            return new Mesh(vertexBuffer.ToArray(), optTriangles);
        }

        private class Triangle
        {
            public readonly int A;
            public readonly int B;
            public readonly int C;

            public Color Color = Color.White;

            public Triangle(int a, int b, int c)
            {
                A = a;
                B = b;
                C = c;
            }
        }
    }
}