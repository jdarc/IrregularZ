namespace IrregularZ
{
    public sealed class Box
    {
        private float _maxX;
        private float _maxY;
        private float _maxZ;
        private float _minX;
        private float _minY;
        private float _minZ;

        public Box()
        {
            Reset();
        }

        public void Reset()
        {
            _minX = float.PositiveInfinity;
            _minY = float.PositiveInfinity;
            _minZ = float.PositiveInfinity;
            _maxX = float.NegativeInfinity;
            _maxY = float.NegativeInfinity;
            _maxZ = float.NegativeInfinity;
        }

        public void Aggregate(float x, float y, float z)
        {
            if (x < _minX) _minX = x;
            if (x > _maxX) _maxX = x;
            if (y < _minY) _minY = y;
            if (y > _maxY) _maxY = y;
            if (z < _minZ) _minZ = z;
            if (z > _maxZ) _maxZ = z;
        }

        public void Aggregate(Box other)
        {
            Aggregate(other._minX, other._minY, other._minZ);
            Aggregate(other._maxX, other._maxY, other._maxZ);
        }

        public int Evaluate(ref Vector3F normal, float distance)
        {
            var l = 0;
            if (normal.X * _minX + normal.Y * _maxY + normal.Z * _minZ - distance > 0) l++;
            if (normal.X * _maxX + normal.Y * _maxY + normal.Z * _minZ - distance > 0) l++;
            if (normal.X * _maxX + normal.Y * _minY + normal.Z * _minZ - distance > 0) l++;
            if (normal.X * _minX + normal.Y * _minY + normal.Z * _minZ - distance > 0) l++;
            if (normal.X * _minX + normal.Y * _maxY + normal.Z * _maxZ - distance > 0) l++;
            if (normal.X * _maxX + normal.Y * _maxY + normal.Z * _maxZ - distance > 0) l++;
            if (normal.X * _maxX + normal.Y * _minY + normal.Z * _maxZ - distance > 0) l++;
            if (normal.X * _minX + normal.Y * _minY + normal.Z * _maxZ - distance > 0) l++;
            return l;
        }

        public void Transform(Box source, ref Matrix4F matrix)
        {
            var minX = source._minX;
            var minY = source._minY;
            var minZ = source._minZ;
            var maxX = source._maxX;
            var maxY = source._maxY;
            var maxZ = source._maxZ;
            AggTran(minX, maxY, minZ, ref matrix);
            AggTran(maxX, maxY, minZ, ref matrix);
            AggTran(maxX, minY, minZ, ref matrix);
            AggTran(minX, minY, minZ, ref matrix);
            AggTran(minX, maxY, maxZ, ref matrix);
            AggTran(maxX, maxY, maxZ, ref matrix);
            AggTran(maxX, minY, maxZ, ref matrix);
            AggTran(minX, minY, maxZ, ref matrix);
        }

        private void AggTran(float x, float y, float z, ref Matrix4F m)
        {
            var w = 1.0f / (m.E41 * x + m.E42 * y + m.E43 * z + m.E44);
            Aggregate(w * (m.E11 * x + m.E12 * y + m.E13 * z + m.E14),
                w * (m.E21 * x + m.E22 * y + m.E23 * z + m.E24),
                w * (m.E31 * x + m.E32 * y + m.E33 * z + m.E34));
        }
    }
}