using Microsoft.Xna.Framework.Input;

namespace IrregularZ
{
    public class FpsCamera
    {
        private const int MovementForward = 1;
        private const int MovementBack = 2;
        private const int MovementLeft = 4;
        private const int MovementRight = 8;

        private readonly Camera _camera;
        private bool _dragging;
        private int _lastMouseXPos;
        private int _lastMouseYPos;
        private Vector3F _lookAt;
        private int _movementMask;
        private float _pitch;
        private float _yaw;

        public FpsCamera(Camera camera)
        {
            _camera = camera;
            _lookAt = _camera.Target;
            _lookAt.Sub(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);
        }

        public void KeyUp(Keys code)
        {
            switch (code)
            {
                case Keys.W:
                    _movementMask &= ~MovementForward;
                    break;
                case Keys.S:
                    _movementMask &= ~MovementBack;
                    break;
                case Keys.A:
                    _movementMask &= ~MovementLeft;
                    break;
                case Keys.D:
                    _movementMask &= ~MovementRight;
                    break;
            }
        }

        public void KeyDown(Keys code)
        {
            switch (code)
            {
                case Keys.W:
                    _movementMask |= MovementForward;
                    break;
                case Keys.S:
                    _movementMask |= MovementBack;
                    break;
                case Keys.A:
                    _movementMask |= MovementLeft;
                    break;
                case Keys.D:
                    _movementMask |= MovementRight;
                    break;
            }
        }

        public void MouseDown()
        {
            _dragging = true;
        }

        public void MouseUp()
        {
            _dragging = false;
        }

        public void MouseMove(int x, int y)
        {
            if (_dragging)
            {
                _yaw += 0.01f * (_lastMouseXPos - x);
                _pitch += 0.01f * (_lastMouseYPos - y);
                _pitch = Function.Clamp(_pitch, -1.56f, 1.56f);
                var matrix = Matrix4F.CreateRotationAboutY(_yaw);
                var rotationAboutX = Matrix4F.CreateRotationAboutX(_pitch);
                matrix.Multiply(ref rotationAboutX);
                _lookAt.Set(0, 0, 1);
                _lookAt.Transform(ref matrix);
            }

            _lastMouseXPos = x;
            _lastMouseYPos = y;
        }

        public void Update(double seconds, float speed)
        {
            var la = _lookAt;
            la.Normalize();

            var camPos = _camera.Position;

            var scaledSpeed = (float) seconds * speed;

            if ((_movementMask & MovementForward) == MovementForward)
                camPos.Add(la.X * scaledSpeed, la.Y * scaledSpeed, la.Z * scaledSpeed);
            else if ((_movementMask & MovementBack) == MovementBack)
                camPos.Sub(la.X * scaledSpeed, la.Y * scaledSpeed, la.Z * scaledSpeed);

            var side = new Vector3F(0, 1, 0);

            side.Cross(ref side, ref la);
            side.Normalize();
            side.Mul(scaledSpeed);
            if ((_movementMask & MovementLeft) == MovementLeft)
                camPos.Sub(side.X, side.Y, side.Z);
            else if ((_movementMask & MovementRight) == MovementRight) camPos.Add(side.X, side.Y, side.Z);

            _camera.MoveTo(camPos.X, camPos.Y, camPos.Z);
            _camera.LookAt(camPos.X + la.X, camPos.Y + la.Y, camPos.Z + la.Z);
        }
    }
}