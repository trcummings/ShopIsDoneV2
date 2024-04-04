using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class HoveringDownStockState : DownStockState
    {
        [Export]
        private RayCast3D _StockRaycast;

        [Export]
        private CharacterBody2D _GrabHand;

        [Export]
        private Camera3D _Camera3D;

        // Speed and acceleration
        private float _MaxSpeed = 800;
        private float _Acceleration = 1200;
        private float _Friction = 0.95f;

        private Area3D _HoveredStockItem = null;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // If we're hovering an item and we press and hold, transition to
            // dragging item state
        }

        public override void PhysicsUpdateState(double delta)
        {
            base.PhysicsUpdateState(delta);

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
                    CalculateVelocity(moveDir.X, _GrabHand.Velocity.X, (float)delta),
                    CalculateVelocity(moveDir.Y, _GrabHand.Velocity.Y, (float)delta)
                );
            }
            _GrabHand.MoveAndSlide();

            // Move around the raycast attached to the hand
            var from = _Camera3D.ProjectRayOrigin(_GrabHand.GlobalPosition);
            var to = from + _Camera3D.ProjectRayNormal(_GrabHand.GlobalPosition) * 4444;
            _StockRaycast.GlobalPosition = from;
            _StockRaycast.TargetPosition = to;

            // If raycast is colliding, set that stock item to the hovered stock
            // item and activate its jiggle
            var collider = _StockRaycast.GetCollider();
            if (_StockRaycast.IsColliding() && collider is Area3D stockItem)
            {
                _HoveredStockItem = stockItem;
                // TODO: Hover anim
            }
            // Otherwise, if we have a hovered stock item, stop it jiggling
            else if (_HoveredStockItem != null && _HoveredStockItem != collider)
            {
                _HoveredStockItem = null;
                // TODO: Unhover anim
            }
        }

        private float CalculateVelocity(float dir, float v, float t)
        {
            if (Mathf.Abs(v) >= _MaxSpeed) return v * _Friction;
            return (dir * _MaxSpeed) + (_Acceleration * t);
        }
    }
}
