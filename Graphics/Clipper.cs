namespace IrregularZ.Graphics
{
    public sealed class Clipper
    {
        private readonly Frustum _frustum;

        public readonly Vector3[] Vertices = new Vector3[8];

        public Clipper(Frustum frustum) => _frustum = frustum;

        public unsafe int Clip()
        {
            var clipMask = ComputeMask();
            if (clipMask == -1) return 0;
            if (clipMask == 0) return 3;

            var aCount = 3;
            var bCount = 0;
            var a = stackalloc Vector3[8];
            var b = stackalloc Vector3[8];

            a[0] = Vertices[0];
            a[1] = Vertices[1];
            a[2] = Vertices[2];
            a[3] = a[0];

            byte mask = 1;
            for (var i = 0; i < _frustum.Planes.Length; ++i)
            {
                if ((clipMask & mask) == mask)
                {
                    var normal = _frustum.Planes[i].Normal;
                    var distance = _frustum.Planes[i].Distance;
                    var a1 = a;
                    var f1 = normal.X * a1->X + normal.Y * a1->Y + normal.Z * a1->Z;
                    for (var v = 1; v <= aCount; ++v)
                    {
                        var a2 = a + v;
                        var f2 = normal.X * a2->X + normal.Y * a2->Y + normal.Z * a2->Z;
                        var dot = -f1 + distance;
                        if (dot > 0)
                        {
                            if (f2 - distance < 0)
                            {
                                *(b + bCount++) = *a2;
                            }
                            else
                            {
                                dot /= f2 - f1;
                                *(b + bCount++) = new Vector3(
                                    a1->X + (a2->X - a1->X) * dot,
                                    a1->Y + (a2->Y - a1->Y) * dot,
                                    a1->Z + (a2->Z - a1->Z) * dot
                                );
                            }
                        }
                        else
                        {
                            if (f2 - distance < 0)
                            {
                                dot /= f2 - f1;
                                *(b + bCount++) = new Vector3(
                                    a1->X + (a2->X - a1->X) * dot,
                                    a1->Y + (a2->Y - a1->Y) * dot,
                                    a1->Z + (a2->Z - a1->Z) * dot
                                );
                                *(b + bCount++) = *a2;
                            }
                        }

                        a1 = a2;
                        f1 = f2;
                    }

                    if (bCount < 3) return 0;

                    *(b + bCount) = *b;
                    aCount = bCount;
                    bCount = 0;
                    var t = a;
                    a = b;
                    b = t;
                }

                mask <<= 1;
            }

            for (var vi = 0; vi < aCount; ++vi) Vertices[vi] = a[vi];
            return aCount;
        }

        private int ComputeMask()
        {
            var v0 = Vertices[0];
            var v1 = Vertices[1];
            var v2 = Vertices[2];
            var clipMask = 0;
            for (var i = 0; i < _frustum.Planes.Length; ++i)
            {
                var p = 0;
                var mask = 1 << i;
                if (_frustum.Planes[i].Dot(v0.X, v0.Y, v0.Z) > 0F)
                {
                    ++p;
                    clipMask |= mask;
                }

                if (_frustum.Planes[i].Dot(v1.X, v1.Y, v1.Z) > 0F)
                {
                    ++p;
                    clipMask |= mask;
                }

                if (_frustum.Planes[i].Dot(v2.X, v2.Y, v2.Z) > 0F)
                {
                    if (++p == 3) return -1;
                    clipMask |= mask;
                }
            }

            return clipMask;
        }
    }
}