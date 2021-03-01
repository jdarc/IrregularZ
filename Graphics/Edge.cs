using System;

namespace IrregularZ.Graphics
{
    internal struct Edge
    {
        public int y;
        public int height;
        public float X;
        public float Z;
        public float XStep;
        public float ZStep;

        public int Configure(ref Tuple3 va, ref Tuple3 vb, ref Gradients gradients)
        {
            y = (int) MathF.Ceiling(va.Y);
            height = (int) MathF.Ceiling(vb.Y) - y;

            XStep = (vb.X - va.X) / (vb.Y - va.Y);
            ZStep = XStep * gradients.dZdX + gradients.dZdY;

            Z = y - va.Y;
            X = XStep * Z + va.X;
            Z = Z * gradients.dZdY + (X - va.X) * gradients.dZdX + va.Z;

            return height;
        }
    }
}