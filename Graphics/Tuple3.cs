using System;

namespace IrregularZ.Graphics
{
    public struct Tuple3 : IEquatable<Tuple3>
    {
        public float X;
        public float Y;
        public float Z;

        public Tuple3(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Tuple3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Tuple3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(Tuple3 left, Tuple3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Tuple3 left, Tuple3 right)
        {
            return !(left == right);
        }
    }
}