using System;
using Microsoft.Xna.Framework.Input;

namespace IrregularZ.Graphics
{
    public class FirstPersonControl
    {
        private const int MovementForward = 1;
        private const int MovementBack = 2;
        private const int MovementLeft = 4;
        private const int MovementRight = 8;

        private readonly Camera _camera;
        private bool _dragging;
        private int _lastMouseXPos;
        private int _lastMouseYPos;
        private Vector3 _lookAt;
        private int _movementMask;
        private float _pitch;
        private float _yaw;

        public FirstPersonControl(Camera camera)
        {
            _camera = camera;
            _lookAt = Vector3.Normalize(_camera.Target - _camera.Position);
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
                _pitch = Math.Clamp(_pitch, -1.56f, 1.56f);
                _lookAt = Matrix4.CreateRotationY(_yaw) * Matrix4.CreateRotationX(_pitch) * new Vector3(0, 0, 1);
            }

            _lastMouseXPos = x;
            _lastMouseYPos = y;
        }

        public void Update(double seconds, float speed)
        {
            var target = Vector3.Normalize(_lookAt);
            var position = _camera.Position;

            var scaledSpeed = (float) seconds * speed;

            if ((_movementMask & MovementForward) == MovementForward)
            {
                position += target * scaledSpeed;
            }
            else if ((_movementMask & MovementBack) == MovementBack)
            {
                position -= target * scaledSpeed;
            }

            var side = Vector3.Normalize(new Vector3(0, 1, 0) * target) * scaledSpeed;
            if ((_movementMask & MovementLeft) == MovementLeft)
            {
                position -= side;
            }
            else if ((_movementMask & MovementRight) == MovementRight)
            {
                position += side;
            }

            _camera.MoveTo(position.X, position.Y, position.Z);
            _camera.LookAt(position.X + target.X, position.Y + target.Y, position.Z + target.Z);
        }
    }
}