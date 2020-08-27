using System.Collections.Generic;

namespace IrregularZ
{
    public sealed class Model : IGeometry, IMaterialModifier
    {
        private readonly Dictionary<string, List<Triangle>> _groups;
        private readonly List<Triangle> _triangles;
        private readonly List<Vector3F> _vertices;
        private List<Triangle> _currentGroup;
        private string _currentGroupName;
        private Dictionary<Material, Mesh> _meshByMaterial;

        public Model()
        {
            _vertices = new List<Vector3F>();
            _triangles = new List<Triangle>();
            _groups = new Dictionary<string, List<Triangle>>();
            BoundingBox = new Box();
            CurrentGroup = "";
        }

        public IEnumerable<string> Groups => _groups.Keys;

        public string CurrentGroup
        {
            get => _currentGroupName;
            set
            {
                _currentGroupName = value;
                if (!_groups.ContainsKey(_currentGroupName)) _groups.Add(_currentGroupName, new List<Triangle>());
                _currentGroup = _groups[_currentGroupName];
            }
        }

        public int VertexCount => _vertices.Count;

        public int TriangleCount => _triangles.Count;

        public Box BoundingBox { get; }

        public void Render(IVisualizer visualizer)
        {
            foreach ((var key, var value) in _meshByMaterial) visualizer.Draw(value, key);
        }

        public IMaterialModifier ChangeMaterial(Material mat)
        {
            _triangles.ForEach(triangle => triangle.ChangeMaterial(mat));
            return this;
        }

        public void AddVertex(float x, float y, float z)
        {
            _vertices.Add(new Vector3F(x, y, z));
        }

        public IMaterialModifier CreateTriangle(int a, int b, int c)
        {
            var triangle = new Triangle(a, b, c);
            _triangles.Add(triangle);
            _currentGroup.Add(triangle);
            return triangle;
        }

        public Model Compile(bool optimize)
        {
            BoundingBox.Reset();
            foreach (var v in _vertices) BoundingBox.Aggregate(v.X, v.Y, v.Z);

            foreach (var triangle in _triangles)
            {
                var a = _vertices[triangle.B] - _vertices[triangle.A];
                var b = _vertices[triangle.C] - _vertices[triangle.B];
                triangle.SurfaceNormal = a * b;
                triangle.SurfaceNormal.Normalize();
            }

            _meshByMaterial = CompileBuffers(optimize);
            return this;
        }

        private Dictionary<Material, Mesh> CompileBuffers(bool optimize)
        {
            var matBuckets = new Dictionary<Material, List<Triangle>>();
            foreach (var triangle in _triangles)
            {
                if (!matBuckets.ContainsKey(triangle.Material)) matBuckets.Add(triangle.Material, new List<Triangle>());
                matBuckets[triangle.Material].Add(triangle);
            }

            var meshByMaterial = new Dictionary<Material, Mesh>();
            foreach (var bucket in matBuckets)
            {
                var triangles = bucket.Value;
                var elementCount = triangles.Count * 3;
                var ib = new int[elementCount];
                var vb = new Vector3F[elementCount];
                var idx = 0;
                foreach (var triangle in triangles)
                {
                    vb[idx] = _vertices[triangle.A];
                    ib[idx] = idx++;

                    vb[idx] = _vertices[triangle.B];
                    ib[idx] = idx++;

                    vb[idx] = _vertices[triangle.C];
                    ib[idx] = idx++;
                }

                var mesh = new Mesh(vb, ib);
                meshByMaterial.Add(bucket.Key, optimize ? mesh.Optimize() : mesh);
            }

            return meshByMaterial;
        }

        private class Triangle : IMaterialModifier
        {
            public readonly int A;
            public readonly int B;
            public readonly int C;
            public Material Material = new Material();
            public Vector3F SurfaceNormal = new Vector3F(0, 0, 0);

            public Triangle(int v0, int v1, int v2)
            {
                A = v0;
                B = v1;
                C = v2;
            }

            public IMaterialModifier ChangeMaterial(Material mat)
            {
                Material = mat;
                return this;
            }
        }
    }
}