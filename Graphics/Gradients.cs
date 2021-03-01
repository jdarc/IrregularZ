namespace IrregularZ.Graphics
{
    public class Gradients
    {
        public float dZdX;
        public float dZdY;

        public Gradients(ref Tuple3 s0, ref Tuple3 s1, ref Tuple3 s2)
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