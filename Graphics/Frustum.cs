namespace IrregularZ.Graphics
{
    public struct Frustum
    {
        public readonly Plane[] Planes;
        private readonly Plane _near;
        private readonly Plane _far;
        private readonly Plane _left;
        private readonly Plane _right;
        private readonly Plane _top;
        private readonly Plane _bottom;

        public Frustum(Matrix4 view, Matrix4 proj)
        {
            var mat = proj * view;
            Planes = new[]
            {
                _near = Plane.Create(-mat.M41 - mat.M31, -mat.M42 - mat.M32, -mat.M43 - mat.M33, mat.M44 + mat.M34),
                _far = Plane.Create(mat.M31 - mat.M41, mat.M32 - mat.M42, mat.M33 - mat.M43, mat.M44 - mat.M34),
                _left = Plane.Create(-mat.M41 - mat.M11, -mat.M42 - mat.M12, -mat.M43 - mat.M13, mat.M44 + mat.M14),
                _right = Plane.Create(mat.M11 - mat.M41, mat.M12 - mat.M42, mat.M13 - mat.M43, mat.M44 - mat.M14),
                _top = Plane.Create(-mat.M41 - mat.M21, -mat.M42 - mat.M22, -mat.M43 - mat.M23, mat.M44 + mat.M24),
                _bottom = Plane.Create(mat.M21 - mat.M41, mat.M22 - mat.M42, mat.M23 - mat.M43, mat.M44 - mat.M24)
            };
        }

        public Containment Evaluate(Aabb aabb)
        {
            var vc0 = aabb.Evaluate(_near);
            if (vc0 == 8) return Containment.Outside;

            var vc1 = aabb.Evaluate(_far);
            if (vc1 == 8) return Containment.Outside;

            var vc2 = aabb.Evaluate(_left);
            if (vc2 == 8) return Containment.Outside;

            var vc3 = aabb.Evaluate(_right);
            if (vc3 == 8) return Containment.Outside;

            var vc4 = aabb.Evaluate(_top);
            if (vc4 == 8) return Containment.Outside;

            var vc5 = aabb.Evaluate(_bottom);
            if (vc5 == 8) return Containment.Outside;

            return vc0 + vc1 + vc2 + vc3 + vc4 + vc5 == 0 ? Containment.Inside : Containment.Partial;
        }
    }
}