using System;
using Godot;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Microgames.DownStock
{
	public partial class DownStockState : State
	{
        [Export]
        protected GrabHand _GrabHand;

        // Speed and acceleration
        private float _MaxSpeed = 1000;
        private float _Acceleration = 1200;
        private float _Friction = 0.95f;

        protected void UpdateHandPosition(float delta)
        {
            // Move hand
            var moveDir = new Vector2(
                Input.GetAxis("fps_move_left", "fps_move_right"),
                Input.GetAxis("fps_move_backward", "fps_move_forward")
            ).Normalized();

            // Flip move dir in y direction
            moveDir.Y = -moveDir.Y;

            // Update velocity
            if (moveDir == Vector2.Zero)
            {
                _GrabHand.Velocity = Vector2.Zero;
            }
            else
            {
                _GrabHand.Velocity = new Vector2(
                    CalculateVelocity(moveDir.X, _GrabHand.Velocity.X, delta),
                    CalculateVelocity(moveDir.Y, _GrabHand.Velocity.Y, delta)
                );
            }
            _GrabHand.MoveAndSlide();
        }

        protected float CalculateVelocity(float dir, float v, float t)
        {
            if (Mathf.Abs(v) >= _MaxSpeed) return v * _Friction;
            return (dir * _MaxSpeed) + (_Acceleration * t);
        }
    }
}

