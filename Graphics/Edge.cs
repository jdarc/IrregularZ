using System;

namespace IrregularZ.Graphics
{
    internal struct Edge
    {
        public int Y;
        public int Height;
        public float X;
        public float Z;
        public float XStep;
        public float ZStep;

        public int Configure(in Vector3 a, in Vector3 b, in Gradients g)
        {
            Y = (int) MathF.Ceiling(a.Y);
            Height = (int) MathF.Ceiling(b.Y) - Y;

            XStep = (b.X - a.X) / (b.Y - a.Y);
            ZStep = XStep * g.dZdX + g.dZdY;

            Z = Y - a.Y;
            X = XStep * Z + a.X;
            Z = Z * g.dZdY + (X - a.X) * g.dZdX + a.Z;

            return Height;
        }
    }
}