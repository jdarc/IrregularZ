using System.Collections.Generic;

namespace IrregularZ
{
    public sealed class Mesh
    {
        public readonly int[] Indices;
        public readonly Vector3F[] Vertices;

        public Mesh(Vector3F[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public Mesh Optimize()
        {
            var vertexMap = new Dictionary<Vector3F, int>();
            var optTriangles = new int[Indices.Length];
            for (var i = 0; i < Indices.Length; i++)
            {
                var va = Vertices[Indices[i]];
                if (!vertexMap.ContainsKey(va)) vertexMap.Add(va, vertexMap.Count);
                optTriangles[i] = vertexMap[va];
            }

            var optVertices = new Vector3F[vertexMap.Count];
            vertexMap.Keys.CopyTo(optVertices, 0);

            return new Mesh(optVertices, optTriangles);
        }
    }
}