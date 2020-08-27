using System;

namespace IrregularZ
{
    public struct Matrix4F
    {
        public static readonly Matrix4F Identity = new Matrix4F(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        public float E11;
        public float E12;
        public float E13;
        public float E14;
        public float E21;
        public float E22;
        public float E23;
        public float E24;
        public float E31;
        public float E32;
        public float E33;
        public float E34;
        public float E41;
        public float E42;
        public float E43;
        public float E44;

        public Matrix4F(float e11, float e12, float e13, float e14,
            float e21, float e22, float e23, float e24,
            float e31, float e32, float e33, float e34,
            float e41, float e42, float e43, float e44)
        {
            E11 = e11;
            E12 = e12;
            E13 = e13;
            E14 = e14;
            E21 = e21;
            E22 = e22;
            E23 = e23;
            E24 = e24;
            E31 = e31;
            E32 = e32;
            E33 = e33;
            E34 = e34;
            E41 = e41;
            E42 = e42;
            E43 = e43;
            E44 = e44;
        }

        public void Set(float e11, float e12, float e13, float e14, float e21, float e22, float e23, float e24,
            float e31, float e32, float e33, float e34, float e41, float e42, float e43, float e44)
        {
            E11 = e11;
            E12 = e12;
            E13 = e13;
            E14 = e14;
            E21 = e21;
            E22 = e22;
            E23 = e23;
            E24 = e24;
            E31 = e31;
            E32 = e32;
            E33 = e33;
            E34 = e34;
            E41 = e41;
            E42 = e42;
            E43 = e43;
            E44 = e44;
        }

        public static Matrix4F operator *(Matrix4F a, Matrix4F b)
        {
            a.Multiply(ref b);
            return a;
        }

        public void Multiply(ref Matrix4F other)
        {
            Multiply(ref this, ref other);
        }

        public void Multiply(ref Matrix4F a, ref Matrix4F b)
        {
            Set(a.E11 * b.E11 + a.E12 * b.E21 + a.E13 * b.E31 + a.E14 * b.E41,
                a.E11 * b.E12 + a.E12 * b.E22 + a.E13 * b.E32 + a.E14 * b.E42,
                a.E11 * b.E13 + a.E12 * b.E23 + a.E13 * b.E33 + a.E14 * b.E43,
                a.E11 * b.E14 + a.E12 * b.E24 + a.E13 * b.E34 + a.E14 * b.E44,
                a.E21 * b.E11 + a.E22 * b.E21 + a.E23 * b.E31 + a.E24 * b.E41,
                a.E21 * b.E12 + a.E22 * b.E22 + a.E23 * b.E32 + a.E24 * b.E42,
                a.E21 * b.E13 + a.E22 * b.E23 + a.E23 * b.E33 + a.E24 * b.E43,
                a.E21 * b.E14 + a.E22 * b.E24 + a.E23 * b.E34 + a.E24 * b.E44,
                a.E31 * b.E11 + a.E32 * b.E21 + a.E33 * b.E31 + a.E34 * b.E41,
                a.E31 * b.E12 + a.E32 * b.E22 + a.E33 * b.E32 + a.E34 * b.E42,
                a.E31 * b.E13 + a.E32 * b.E23 + a.E33 * b.E33 + a.E34 * b.E43,
                a.E31 * b.E14 + a.E32 * b.E24 + a.E33 * b.E34 + a.E34 * b.E44,
                a.E41 * b.E11 + a.E42 * b.E21 + a.E43 * b.E31 + a.E44 * b.E41,
                a.E41 * b.E12 + a.E42 * b.E22 + a.E43 * b.E32 + a.E44 * b.E42,
                a.E41 * b.E13 + a.E42 * b.E23 + a.E43 * b.E33 + a.E44 * b.E43,
                a.E41 * b.E14 + a.E42 * b.E24 + a.E43 * b.E34 + a.E44 * b.E44);
        }

        public void Invert()
        {
            Invert(ref this);
        }

        public void Invert(ref Matrix4F m)
        {
            float a00 = m.E11, a01 = m.E12, a02 = m.E13, a03 = m.E14;
            float a10 = m.E21, a11 = m.E22, a12 = m.E23, a13 = m.E24;
            float a20 = m.E31, a21 = m.E32, a22 = m.E33, a23 = m.E34;
            float a30 = m.E41, a31 = m.E42, a32 = m.E43, a33 = m.E44;
            var b00 = a00 * a11 - a01 * a10;
            var b01 = a00 * a12 - a02 * a10;
            var b02 = a00 * a13 - a03 * a10;
            var b03 = a01 * a12 - a02 * a11;
            var b04 = a01 * a13 - a03 * a11;
            var b05 = a02 * a13 - a03 * a12;
            var b06 = a20 * a31 - a21 * a30;
            var b07 = a20 * a32 - a22 * a30;
            var b08 = a20 * a33 - a23 * a30;
            var b09 = a21 * a32 - a22 * a31;
            var b10 = a21 * a33 - a23 * a31;
            var b11 = a22 * a33 - a23 * a32;
            var invDet = 1 / (b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06);
            m.E11 = (a11 * b11 - a12 * b10 + a13 * b09) * invDet;
            m.E12 = (-a01 * b11 + a02 * b10 - a03 * b09) * invDet;
            m.E13 = (a31 * b05 - a32 * b04 + a33 * b03) * invDet;
            m.E14 = (-a21 * b05 + a22 * b04 - a23 * b03) * invDet;
            m.E21 = (-a10 * b11 + a12 * b08 - a13 * b07) * invDet;
            m.E22 = (a00 * b11 - a02 * b08 + a03 * b07) * invDet;
            m.E23 = (-a30 * b05 + a32 * b02 - a33 * b01) * invDet;
            m.E24 = (a20 * b05 - a22 * b02 + a23 * b01) * invDet;
            m.E31 = (a10 * b10 - a11 * b08 + a13 * b06) * invDet;
            m.E32 = (-a00 * b10 + a01 * b08 - a03 * b06) * invDet;
            m.E33 = (a30 * b04 - a31 * b02 + a33 * b00) * invDet;
            m.E34 = (-a20 * b04 + a21 * b02 - a23 * b00) * invDet;
            m.E41 = (-a10 * b09 + a11 * b07 - a12 * b06) * invDet;
            m.E42 = (a00 * b09 - a01 * b07 + a02 * b06) * invDet;
            m.E43 = (-a30 * b03 + a31 * b01 - a32 * b00) * invDet;
            m.E44 = (a20 * b03 - a21 * b01 + a22 * b00) * invDet;
        }

        public static Matrix4F CreateRotationAboutX(float angle)
        {
            var c = (float) Math.Cos(angle);
            var s = (float) Math.Sin(angle);
            return new Matrix4F(1, 0, 0, 0, 0, c, s, 0, 0, -s, c, 0, 0, 0, 0, 1);
        }

        public static Matrix4F CreateRotationAboutY(float angle)
        {
            var c = (float) Math.Cos(angle);
            var s = (float) Math.Sin(angle);
            return new Matrix4F(c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, 0, 0, 0, 1);
        }

        public static Matrix4F CreateRotationAboutZ(float angle)
        {
            var c = (float) Math.Cos(angle);
            var s = (float) Math.Sin(angle);
            return new Matrix4F(c, s, 0, 0, -s, c, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        }

        public static Matrix4F CreateScaler(float x, float y, float z)
        {
            return new Matrix4F(x, 0, 0, 0, 0, y, 0, 0, 0, 0, z, 0, 0, 0, 0, 1);
        }

        public static Matrix4F CreateScaler(float scale)
        {
            return CreateScaler(scale, scale, scale);
        }

        public static Matrix4F CreateTranslation(float x, float y, float z)
        {
            return new Matrix4F(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z, 0, 0, 0, 1);
        }
    }
}