using System;

namespace IrregularZ
{
    public sealed class Camera
    {
        private static readonly Vector3F UpVector = new Vector3F(0, 1, 0);
        private readonly float _aspectRatio;
        private readonly float _far;
        private readonly float _near;
        private readonly Matrix4F _projMatrix;
        private Vector3F _cameraPosition = new Vector3F(0, 0, 1);
        private bool _dirty;
        private Frustum _frustum;
        private Vector3F _targetPosition = new Vector3F(0, 0, 0);
        private Matrix4F _viewMatrix = Matrix4F.Identity;

        public Camera(float fov, int width, int height, float near, float far)
        {
            _near = near;
            _far = far >= near ? far : near;
            _aspectRatio = width / (float) height;
            _projMatrix.E22 = (float) (1.0 / Math.Tan(fov / 2.0));
            _projMatrix.E11 = _projMatrix.E22 / _aspectRatio;
            _projMatrix.E33 = far / (far - near);
            _projMatrix.E34 = -_projMatrix.E33 * near;
            _projMatrix.E43 = 1.0f;
            _dirty = true;
        }

        public Vector3F Position => _cameraPosition;

        public Vector3F Target => _targetPosition;

        public Matrix4F ViewMatrix => CalculateViewMatrix();

        public Matrix4F ProjectionMatrix => _projMatrix;

        public Frustum Frustum
        {
            get
            {
                CalculateViewMatrix();
                return _frustum;
            }
        }

        public void MoveTo(float x, float y, float z)
        {
            _cameraPosition.Set(x, y, z);
            _dirty = true;
        }

        public void LookAt(float x, float y, float z)
        {
            _targetPosition.Set(x, y, z);
            _dirty = true;
        }

        private Matrix4F CalculateViewMatrix()
        {
            if (!_dirty) return _viewMatrix;
            var nVector = _targetPosition - _cameraPosition;
            nVector.Normalize();

            var vVector = UpVector * nVector;
            vVector.Normalize();

            var uVector = vVector * nVector;
            uVector.Normalize();

            _viewMatrix.E11 = vVector.X;
            _viewMatrix.E12 = vVector.Y;
            _viewMatrix.E13 = vVector.Z;
            _viewMatrix.E14 = -_cameraPosition.Dot(ref vVector);

            _viewMatrix.E21 = uVector.X;
            _viewMatrix.E22 = uVector.Y;
            _viewMatrix.E23 = uVector.Z;
            _viewMatrix.E24 = -_cameraPosition.Dot(ref uVector);

            _viewMatrix.E31 = nVector.X;
            _viewMatrix.E32 = nVector.Y;
            _viewMatrix.E33 = nVector.Z;
            _viewMatrix.E34 = -_cameraPosition.Dot(ref nVector);

            // Compute frustum normals
            var nearPoint = _cameraPosition + _near * nVector;
            var farPoint = _cameraPosition + _far * nVector;

            vVector *= _aspectRatio;
            nVector *= _projMatrix.E22;

            var temp = nVector - uVector + vVector;

            _frustum.Right.Normal = temp * uVector;
            _frustum.Right.Distance = _cameraPosition.Dot(ref _frustum.Right.Normal);

            _frustum.Top.Normal = temp * vVector;
            _frustum.Top.Distance = _cameraPosition.Dot(ref _frustum.Top.Normal);

            temp = nVector + uVector - vVector;

            _frustum.Left.Normal = uVector * temp;
            _frustum.Left.Distance = _cameraPosition.Dot(ref _frustum.Left.Normal);

            _frustum.Bottom.Normal = vVector * temp;
            _frustum.Bottom.Distance = _cameraPosition.Dot(ref _frustum.Bottom.Normal);

            _frustum.Near.Normal = -nVector;
            _frustum.Near.Distance = nearPoint.Dot(ref _frustum.Near.Normal);

            _frustum.Far.Normal = nVector;
            _frustum.Far.Distance = farPoint.Dot(ref _frustum.Far.Normal);

            _dirty = false;

            return _viewMatrix;
        }
    }
}