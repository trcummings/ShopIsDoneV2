using Godot;
using System;

namespace ShopIsDone.Microgames.HallwayChase
{
    public partial class MovementController : Node
    {
        [Export]
        public float Speed = 10;

        [Export]
        public float Acceleration = 8;

        [Export]
        public float Deceleration = 10;

        [Export]
        public float AirControl = 0.3f;

        [Export]
        private CharacterBody3D _Body;

        private Vector3 _Direction;
        private Vector3 _Velocity;
        private float _Gravity = 9.8f;

        public Vector3 Velocity { get { return _Velocity; } }

        public bool IsMoving()
        {
            return Mathf.Abs(_Velocity.X) > 0.1 || Mathf.Abs(_Velocity.Z) > 0.1;
        }

        public void Move(Vector2 moveInput, float delta)
        {
            // Calculate direction based on facing basis
            _Direction = Vector3.Zero;
            var aim = _Body.GlobalTransform.Basis;
            _Direction = (aim.Z * -moveInput.X) + (aim.X * moveInput.Y);

            // Handle if we're in the air
            if (_Body.IsOnFloor())
            {
                //_Snap = -_Body.GetFloorNormal() - _Body.GetPlatformVelocity() * delta;
                // Workaround for sliding down after a fall onto a slope
                if (_Velocity.Y < 0) _Velocity.Y = 0;
            }
            else
            {
                // Workaround for "vertical bump" when going off a platform
                //if (_Snap != Vector3.Zero && _Velocity.Y != 0) _Velocity.Y = 0;
                //_Snap = Vector3.Zero;
                // Apply Gravity
                _Velocity.Y -= _Gravity * delta;
            }

            // Handle Acceleration
            var tempVelocity = _Velocity;
            // Only use the horizontal velocity to interpolate towards the input
            tempVelocity.Y = 0;

            float tempAccel;
            var target = _Direction * Speed;

            // Decide if we're accelerating or decelerating
            if (_Direction.Dot(tempVelocity) > 0) tempAccel = Acceleration;
            else tempAccel = Deceleration;

            // Apply air control
            if (!_Body.IsOnFloor()) tempAccel *= AirControl;

            // Interpolate velocity
            tempVelocity = tempVelocity.Lerp(target, tempAccel * delta);
            _Velocity.X = tempVelocity.X;
            _Velocity.Z = tempVelocity.Z;
            _Body.Velocity = _Velocity;

            // Apply velocity and move
            _Body.MoveAndSlide();
            _Velocity = _Body.Velocity;
        }
    }
}