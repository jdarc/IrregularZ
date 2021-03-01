namespace IrregularZ.Graphics
{
    public readonly struct Gradients
    {
        public readonly float dZdX;
        public readonly float dZdY;

        public Gradients(in Vector3 s0, in Vector3 s1, in Vector3 s2)
        {
            var ax = s0.X - s2.X;
            var ay = s0.Y - s2.Y;
            var az = s0.Z - s2.Z;
            var bx = s1.X - s2.X;
            var by = s1.Y - s2.Y;
            var bz = s1.Z - s2.Z;
            var oneOverDx = 1F / (bx * ay - ax * by);
            dZdX = (bz * ay - az * by) * oneOverDx;
            dZdY = (az * bx - bz * ax) * oneOverDx;
        }
    }
}