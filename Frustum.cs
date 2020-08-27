namespace IrregularZ
{
    public struct Frustum
    {
        public Plane Near;
        public Plane Far;
        public Plane Left;
        public Plane Right;
        public Plane Top;
        public Plane Bottom;

        public Containment Evaluate(Box box)
        {
            var vc0 = box.Evaluate(ref Near.Normal, Near.Distance);
            if (vc0 == 8) return Containment.Outside;

            var vc1 = box.Evaluate(ref Far.Normal, Far.Distance);
            if (vc1 == 8) return Containment.Outside;

            var vc2 = box.Evaluate(ref Left.Normal, Left.Distance);
            if (vc2 == 8) return Containment.Outside;

            var vc3 = box.Evaluate(ref Right.Normal, Right.Distance);
            if (vc3 == 8) return Containment.Outside;

            var vc4 = box.Evaluate(ref Top.Normal, Top.Distance);
            if (vc4 == 8) return Containment.Outside;

            var vc5 = box.Evaluate(ref Bottom.Normal, Bottom.Distance);
            if (vc5 == 8) return Containment.Outside;

            return vc0 + vc1 + vc2 + vc3 + vc4 + vc5 == 0 ? Containment.Inside : Containment.Partial;
        }
    }
}