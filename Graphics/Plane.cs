using System;

namespace IrregularZ.Graphics
{
    public readonly struct Plane
    {
        public readonly Vector3 Normal;
        public readonly float Distance;

        private Plane(Vector3 norm, float dist)
        {
            Normal = norm;
            Distance = dist;
        }

        public float Dot(float x, float y, float z) => Normal.X * x + Normal.Y * y + Normal.Z * z - Distance;

        public static Plane Create(float x, float y, float z, float d)
        {
            var invLen = 1F / MathF.Sqrt(x * x + y * y + z * z);
            return new Plane(new Vector3(x * invLen, y * invLen, z * invLen), d * invLen);
        }
    }
}