namespace IrregularZ.Graphics
{
    public sealed class Camera
    {
        private static readonly Vector3 UpVector = new Vector3(0, 1, 0);
        private readonly float _aspectRatio;
        private readonly float _far;
        private readonly float _fov;
        private readonly float _near;

        public Camera(float fov, int width, int height, float near, float far)
        {
            _fov = fov;
            _near = near;
            _far = far >= near ? far : near;
            _aspectRatio = width / (float) height;
        }

        public Vector3 Position { get; private set; } = new Vector3(0, 0, 1);

        public Vector3 Target { get; private set; } = new Vector3(0, 0, 0);

        public Matrix4 ViewMatrix => Matrix4.CreateLookAt(Position, Target, UpVector);

        public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFov(_fov, _aspectRatio, _near, _far);

        public void MoveTo(float x, float y, float z) => Position = new Vector3(x, y, z);

        public void LookAt(float x, float y, float z) => Target = new Vector3(x, y, z);
    }
}