using System;

namespace IrregularZ.Graphics
{
    public sealed class Aabb
    {
        private float _maxX;
        private float _maxY;
        private float _maxZ;
        private float _minX;
        private float _minY;
        private float _minZ;

        public Aabb()
        {
            Reset();
        }

        public void Reset()
        {
            _minX = _minY = _minZ = float.PositiveInfinity;
            _maxX = _maxY = _maxZ = float.NegativeInfinity;
        }

        public int Evaluate(in Plane plane)
        {
            return (plane.Dot(_minX, _maxY, _minZ) > 0F ? 1 : 0) +
                   (plane.Dot(_maxX, _maxY, _minZ) > 0F ? 1 : 0) +
                   (plane.Dot(_maxX, _minY, _minZ) > 0F ? 1 : 0) +
                   (plane.Dot(_minX, _minY, _minZ) > 0F ? 1 : 0) +
                   (plane.Dot(_minX, _maxY, _maxZ) > 0F ? 1 : 0) +
                   (plane.Dot(_maxX, _maxY, _maxZ) > 0F ? 1 : 0) +
                   (plane.Dot(_maxX, _minY, _maxZ) > 0F ? 1 : 0) +
                   (plane.Dot(_minX, _minY, _maxZ) > 0F ? 1 : 0);
        }

        public void Aggregate(float x, float y, float z)
        {
            if (float.IsInfinity(x) || float.IsInfinity(y) || float.IsInfinity(z)) return;
            _minX = MathF.Min(x, _minX);
            _minY = MathF.Min(y, _minY);
            _minZ = MathF.Min(z, _minZ);
            _maxX = MathF.Max(x, _maxX);
            _maxY = MathF.Max(y, _maxY);
            _maxZ = MathF.Max(z, _maxZ);
        }

        public void Aggregate(Aabb other)
        {
            Aggregate(other._minX, other._minY, other._minZ);
            Aggregate(other._maxX, other._maxY, other._maxZ);
        }

        public void Aggregate(Aabb source, in Matrix4x4 matrix)
        {
            Aggregate(source._minX, source._maxY, source._minZ, matrix);
            Aggregate(source._maxX, source._maxY, source._minZ, matrix);
            Aggregate(source._maxX, source._minY, source._minZ, matrix);
            Aggregate(source._minX, source._minY, source._minZ, matrix);
            Aggregate(source._minX, source._maxY, source._maxZ, matrix);
            Aggregate(source._maxX, source._maxY, source._maxZ, matrix);
            Aggregate(source._maxX, source._minY, source._maxZ, matrix);
            Aggregate(source._minX, source._minY, source._maxZ, matrix);
        }

        private void Aggregate(float x, float y, float z, in Matrix4x4 m)
        {
            var bx = m.M11 * x + m.M12 * y + m.M13 * z + m.M14;
            var by = m.M21 * x + m.M22 * y + m.M23 * z + m.M24;
            var bz = m.M31 * x + m.M32 * y + m.M33 * z + m.M34;
            var bw = m.M41 * x + m.M42 * y + m.M43 * z + m.M44;
            Aggregate(bx / bw, by / bw, bz / bw);
        }
    }
}