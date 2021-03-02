namespace IrregularZ.Graphics
{
    public struct Frustum
    {
        public Plane Near;
        public Plane Far;
        public Plane Left;
        public Plane Right;
        public Plane Top;
        public Plane Bottom;

        public Frustum(Matrix4 view, Matrix4 proj)
        {
            var mat = proj * view;
            Near = Plane.Create(-mat.M41 - mat.M31, -mat.M42 - mat.M32, -mat.M43 - mat.M33, mat.M44 + mat.M34);
            Far = Plane.Create(mat.M31 - mat.M41, mat.M32 - mat.M42, mat.M33 - mat.M43, mat.M44 - mat.M34);
            Left = Plane.Create(-mat.M41 - mat.M11, -mat.M42 - mat.M12, -mat.M43 - mat.M13, mat.M44 + mat.M14);
            Right = Plane.Create(mat.M11 - mat.M41, mat.M12 - mat.M42, mat.M13 - mat.M43, mat.M44 - mat.M14);
            Top = Plane.Create(-mat.M41 - mat.M21, -mat.M42 - mat.M22, -mat.M43 - mat.M23, mat.M44 + mat.M24);
            Bottom = Plane.Create(mat.M21 - mat.M41, mat.M22 - mat.M42, mat.M23 - mat.M43, mat.M44 - mat.M24);
        }

        public Containment Evaluate(Aabb aabb)
        {
            var vc0 = aabb.Evaluate(Near);
            if (vc0 == 8) return Containment.Outside;

            var vc1 = aabb.Evaluate(Far);
            if (vc1 == 8) return Containment.Outside;

            var vc2 = aabb.Evaluate(Left);
            if (vc2 == 8) return Containment.Outside;

            var vc3 = aabb.Evaluate(Right);
            if (vc3 == 8) return Containment.Outside;

            var vc4 = aabb.Evaluate(Top);
            if (vc4 == 8) return Containment.Outside;

            var vc5 = aabb.Evaluate(Bottom);
            if (vc5 == 8) return Containment.Outside;

            return vc0 + vc1 + vc2 + vc3 + vc4 + vc5 == 0 ? Containment.Inside : Containment.Partial;
        }
    }
}