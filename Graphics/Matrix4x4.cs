using System;
using System.Diagnostics.CodeAnalysis;

namespace IrregularZ.Graphics
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public readonly struct Matrix4x4
    {
        public static readonly Matrix4x4 Identity = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        public readonly float M11;
        public readonly float M12;
        public readonly float M13;
        public readonly float M14;
        public readonly float M21;
        public readonly float M22;
        public readonly float M23;
        public readonly float M24;
        public readonly float M31;
        public readonly float M32;
        public readonly float M33;
        public readonly float M34;
        public readonly float M41;
        public readonly float M42;
        public readonly float M43;
        public readonly float M44;

        public Matrix4x4(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        public static Matrix4x4 Invert(in Matrix4x4 m)
        {
            float a00 = m.M11, a01 = m.M12, a02 = m.M13, a03 = m.M14;
            float a10 = m.M21, a11 = m.M22, a12 = m.M23, a13 = m.M24;
            float a20 = m.M31, a21 = m.M32, a22 = m.M33, a23 = m.M34;
            float a30 = m.M41, a31 = m.M42, a32 = m.M43, a33 = m.M44;
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
            var m11 = invDet * (a11 * b11 - a12 * b10 + a13 * b09);
            var m12 = invDet * (-a01 * b11 + a02 * b10 - a03 * b09);
            var m13 = invDet * (a31 * b05 - a32 * b04 + a33 * b03);
            var m14 = invDet * (-a21 * b05 + a22 * b04 - a23 * b03);
            var m21 = invDet * (-a10 * b11 + a12 * b08 - a13 * b07);
            var m22 = invDet * (a00 * b11 - a02 * b08 + a03 * b07);
            var m23 = invDet * (-a30 * b05 + a32 * b02 - a33 * b01);
            var m24 = invDet * (a20 * b05 - a22 * b02 + a23 * b01);
            var m31 = invDet * (a10 * b10 - a11 * b08 + a13 * b06);
            var m32 = invDet * (-a00 * b10 + a01 * b08 - a03 * b06);
            var m33 = invDet * (a30 * b04 - a31 * b02 + a33 * b00);
            var m34 = invDet * (-a20 * b04 + a21 * b02 - a23 * b00);
            var m41 = invDet * (-a10 * b09 + a11 * b07 - a12 * b06);
            var m42 = invDet * (a00 * b09 - a01 * b07 + a02 * b06);
            var m43 = invDet * (-a30 * b03 + a31 * b01 - a32 * b00);
            var m44 = invDet * (a20 * b03 - a21 * b01 + a22 * b00);
            return new Matrix4x4(m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
        }

        public static Matrix4x4 CreateRotationX(float angle)
        {
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);
            return new Matrix4x4(1, 0, 0, 0, 0, cos, sin, 0, 0, -sin, cos, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4 CreateRotationY(float angle)
        {
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);
            return new Matrix4x4(cos, 0, -sin, 0, 0, 1, 0, 0, sin, 0, cos, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4 CreateScale(float x, float y, float z)
        {
            return new Matrix4x4(x, 0, 0, 0, 0, y, 0, 0, 0, 0, z, 0, 0, 0, 0, 1);
        }

        public static Matrix4x4 CreateTranslation(float x, float y, float z)
        {
            return new Matrix4x4(1, 0, 0, x, 0, 1, 0, y, 0, 0, 1, z, 0, 0, 0, 1);
        }

        public static Matrix4x4 CreateLookAt(Vector3 eye, Vector3 at, Vector3 up)
        {
            var nVector = Vector3.Normalize(at - eye);
            var vVector = Vector3.Normalize(up * nVector);
            var uVector = Vector3.Normalize(vVector * nVector);
            return new Matrix4x4(
                vVector.X, vVector.Y, vVector.Z, -Vector3.Dot(eye, vVector),
                uVector.X, uVector.Y, uVector.Z, -Vector3.Dot(eye, uVector),
                nVector.X, nVector.Y, nVector.Z, -Vector3.Dot(eye, nVector),
                0, 0, 0, 1);
        }

        public static Matrix4x4 CreatePerspectiveFov(float fov, float aspectRatio, float near, float far)
        {
            var m22 = 1F / MathF.Tan(fov / 2F);
            var m11 = m22 / aspectRatio;
            var m33 = far / (far - near);
            var m34 = -(far * near) / (far - near);
            return new Matrix4x4(m11, 0, 0, 0, 0, m22, 0, 0, 0, 0, m33, m34, 0, 0, 1, 0);
        }

        public static Matrix4x4 CreateViewportMatrix(int width, int height)
        {
            var hw = width / 2F;
            var hh = height / 2F;
            return new Matrix4x4(hw, 0, 0, hw - 0.5F, 0, hh, 0, hh - 0.5F, 0, 0, 1, 0, 0, 0, 0, 1);
        }
    }
}