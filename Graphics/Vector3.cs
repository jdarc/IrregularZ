using System;

namespace IrregularZ.Graphics
{
    public readonly struct Vector3 : IEquatable<Vector3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => (float) Math.Sqrt(X * X + Y * Y + Z * Z);

        public bool Equals(Vector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(in Vector3 a, in Vector3 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(in Vector3 a, in Vector3 b)
        {
            return !a.Equals(b);
        }

        public static Vector3 operator +(in Vector3 a, in Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(in Vector3 a, in Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator -(in Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator /(in Vector3 v, in float s)
        {
            return new Vector3(v.X / s, v.Y / s, v.Z / s);
        }

        public static Vector3 operator /(in float s, in Vector3 v)
        {
            return v / s;
        }

        public static Vector3 operator *(in Vector3 v, in float s)
        {
            return new Vector3(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vector3 operator *(in float s, in Vector3 v)
        {
            return v * s;
        }

        public static Vector3 operator *(in Vector3 a, in Vector3 b)
        {
            return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public static Vector3 operator *(in Vector3 position, in Matrix4x4 matrix)
        {
            var x = matrix.M11 * position.X + matrix.M12 * position.Y + matrix.M13 * position.Z + matrix.M14;
            var y = matrix.M21 * position.X + matrix.M22 * position.Y + matrix.M23 * position.Z + matrix.M24;
            var z = matrix.M31 * position.X + matrix.M32 * position.Y + matrix.M33 * position.Z + matrix.M34;
            return new Vector3(x, y, z);
        }

        public static float Dot(in Vector3 a, in Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 Normalize(in Vector3 v)
        {
            return v / v.Length;
        }
    }
}