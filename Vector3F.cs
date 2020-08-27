using System;

namespace IrregularZ
{
    public struct Vector3F : IEquatable<Vector3F>
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float LengthSquared => X * X + Y * Y + Z * Z;

        public float Length => (float) Math.Sqrt(LengthSquared);

        public void Normalize()
        {
            Mul(1.0f / Length);
        }

        public void Sub(float x, float y, float z)
        {
            X -= x;
            Y -= y;
            Z -= z;
        }

        public void Add(float x, float y, float z)
        {
            X += x;
            Y += y;
            Z += z;
        }

        public void Mul(float s)
        {
            X *= s;
            Y *= s;
            Z *= s;
        }

        public float Dot(float x, float y, float z)
        {
            return X * x + Y * y + Z * z;
        }

        public float Dot(ref Vector3F v)
        {
            return Dot(v.X, v.Y, v.Z);
        }

        public void Cross(ref Vector3F a, ref Vector3F b)
        {
            Set(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public void Transform(ref Matrix4F m, ref Vector3F v)
        {
            Set(m.E11 * v.X + m.E12 * v.Y + m.E13 * v.Z + m.E14, m.E21 * v.X + m.E22 * v.Y + m.E23 * v.Z + m.E24,
                m.E31 * v.X + m.E32 * v.Y + m.E33 * v.Z + m.E34);
        }

        public void Transform(ref Matrix4F m)
        {
            Transform(ref m, ref this);
        }

        public bool Equals(Vector3F other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector3F)) return false;
            return Equals((Vector3F) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = X.GetHashCode();
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Vector3F left, Vector3F right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3F left, Vector3F right)
        {
            return !left.Equals(right);
        }

        public static Vector3F operator +(Vector3F a, Vector3F b)
        {
            return new Vector3F(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3F operator -(Vector3F a, Vector3F b)
        {
            return new Vector3F(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3F operator -(Vector3F a)
        {
            return new Vector3F(-a.X, -a.Y, -a.Z);
        }

        public static Vector3F operator *(Vector3F a, float s)
        {
            return new Vector3F(a.X * s, a.Y * s, a.Z * s);
        }

        public static Vector3F operator *(float s, Vector3F a)
        {
            return new Vector3F(a.X * s, a.Y * s, a.Z * s);
        }

        public static Vector3F operator *(Vector3F a, Vector3F b)
        {
            a.Cross(ref a, ref b);
            return a;
        }
    }
}