using System;

namespace IrregularZ.Graphics
{
    public static class Clipper
    {
        public static unsafe void Clip(Frustum frustum, ref Tuple3 v0, ref Tuple3 v1, ref Tuple3 v2)
        {
            byte clipMask = 0;
            int i;
            var planes = &frustum.Near;
            byte mask = 1;
            for (i = 0; i < 6; ++i)
            {
                var p = 0;
                var pl = planes + i;
                if (pl->Normal.X * v0.X + pl->Normal.Y * v0.Y + pl->Normal.Z * v0.Z - pl->Distance > 0)
                {
                    ++p;
                    clipMask |= mask;
                }

                if (pl->Normal.X * v1.X + pl->Normal.Y * v1.Y + pl->Normal.Z * v1.Z - pl->Distance > 0)
                {
                    ++p;
                    clipMask |= mask;
                }

                if (pl->Normal.X * v2.X + pl->Normal.Y * v2.Y + pl->Normal.Z * v2.Z - pl->Distance > 0)
                {
                    if (++p == 3) return;
                    clipMask |= mask;
                }

                mask <<= 1;
            }

            if (clipMask != 0)
            {
                var a = stackalloc Tuple3[8];
                var b = stackalloc Tuple3[8];
                var acount = 3;
                var bcount = 0;

                a[0] = a[3] = v0;
                a[1] = v1;
                a[2] = v2;

                mask = 1;
                for (i = 0; i < 6; ++i)
                {
                    if ((clipMask & mask) == mask)
                    {
                        var pl = planes + i;
                        var pln = &pl->Normal;
                        var pld = pl->Distance;
                        var a1 = a;
                        var f1 = pln->X * a1->X + pln->Y * a1->Y + pln->Z * a1->Z;
                        for (var v = 1; v <= acount; ++v)
                        {
                            var a2 = a + v;
                            var f2 = pln->X * a2->X + pln->Y * a2->Y + pln->Z * a2->Z;
                            var dot = -f1 + pld;
                            if (dot > 0)
                            {
                                if (f2 - pld < 0)
                                {
                                    *(b + bcount++) = *a2;
                                }
                                else
                                {
                                    dot /= f2 - f1;
                                    var target = b + bcount++;
                                    target->X = a1->X + (a2->X - a1->X) * dot;
                                    target->Y = a1->Y + (a2->Y - a1->Y) * dot;
                                    target->Z = a1->Z + (a2->Z - a1->Z) * dot;
                                }
                            }
                            else
                            {
                                if (f2 - pld < 0)
                                {
                                    dot /= f2 - f1;
                                    var target = b + bcount++;
                                    target->X = a1->X + (a2->X - a1->X) * dot;
                                    target->Y = a1->Y + (a2->Y - a1->Y) * dot;
                                    target->Z = a1->Z + (a2->Z - a1->Z) * dot;
                                    *(b + bcount++) = *a2;
                                }
                            }

                            a1 = a2;
                            f1 = f2;
                        }

                        if (bcount < 3) return;

                        *(b + bcount) = *b;
                        acount = bcount;
                        bcount = 0;
                        var t = a;
                        a = b;
                        b = t;
                    }

                    mask <<= 1;
                }

                // if (acount > 3)
                // {
                //     acount -= 2;
                //     var ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                //                    _combMatrix.M44);
                //     s0.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                //                  _combMatrix.M14);
                //     s0.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                //                  _combMatrix.M24);
                //     s0.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                //                  _combMatrix.M34);
                //     ++a;
                //     ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                //                _combMatrix.M44);
                //     s2.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                //                  _combMatrix.M14);
                //     s2.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                //                  _combMatrix.M24);
                //     s2.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                //                  _combMatrix.M34);
                //     do
                //     {
                //         s1 = s2;
                //         ++a;
                //         ow = 1F / (_combMatrix.M41 * a->X + _combMatrix.M42 * a->Y + _combMatrix.M43 * a->Z +
                //                    _combMatrix.M44);
                //         s2.X = ow * (_combMatrix.M11 * a->X + _combMatrix.M12 * a->Y + _combMatrix.M13 * a->Z +
                //                      _combMatrix.M14);
                //         s2.Y = ow * (_combMatrix.M21 * a->X + _combMatrix.M22 * a->Y + _combMatrix.M23 * a->Z +
                //                      _combMatrix.M24);
                //         s2.Z = ow * (_combMatrix.M31 * a->X + _combMatrix.M32 * a->Y + _combMatrix.M33 * a->Z +
                //                      _combMatrix.M34);
                //         ScanOrder(color, ref s0, ref s1, ref s2);
                //     } while (--acount > 0);
                //
                //     return;
                // }

                v0 = *a;
                v1 = *++a;
                v2 = *++a;
            }
        }
    }
}