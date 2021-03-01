using System;
using System.Runtime.CompilerServices;

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

        public bool Equals(Vector3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        public override bool Equals(object obj) => obj is Vector3 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static bool operator ==(in Vector3 a, in Vector3 b) => a.Equals(b);

        public static bool operator !=(in Vector3 a, in Vector3 b) => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(in Vector3 a, in Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 a, in Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(in Vector3 v, in float s) => new Vector3(v.X / s, v.Y / s, v.Z / s);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(in float s, in Vector3 v) => v / s;

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 v, in float s) => new Vector3(v.X * s, v.Y * s, v.Z * s);

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in float s, in Vector3 v) => v * s;

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 a, in Vector3 b)
        {
            var x = a.Y * b.Z - a.Z * b.Y;
            var y = a.Z * b.X - a.X * b.Z;
            var z = a.X * b.Y - a.Y * b.X;
            return new Vector3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Matrix4 matrix, in Vector3 position)
        {
            var x = matrix.M11 * position.X + matrix.M12 * position.Y + matrix.M13 * position.Z + matrix.M14;
            var y = matrix.M21 * position.X + matrix.M22 * position.Y + matrix.M23 * position.Z + matrix.M24;
            var z = matrix.M31 * position.X + matrix.M32 * position.Y + matrix.M33 * position.Z + matrix.M34;
            return new Vector3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector3 a, in Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(in Vector3 v) => v / v.Length;
    }
}