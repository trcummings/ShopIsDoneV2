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
        private Camera3D _Camera3D;

        // Speed and acceleration
        private float _MaxSpeed = 800;
        private float _Acceleration = 1200;
        private float _Friction = 0.95f;

        private StockItem _HoveredStockItem = null;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            _GrabHand.Release();
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // If we're hovering an item and we press and hold, transition to
            // dragging item state
            if (Input.IsActionJustPressed("ui_accept") && _HoveredStockItem != null)
            {
                _HoveredStockItem.Unhover();
                ChangeState(Consts.States.DRAGGING, new Dictionary<string, Variant>()
                {
                    { Consts.HOVERED_ITEM_KEY, _HoveredStockItem }
                });
                return;
            }
        }

        public override void PhysicsUpdateState(double delta)
        {
            base.PhysicsUpdateState(delta);

            // Update hand pos
            UpdateHandPosition((float)delta);

            // Move around the raycast attached to the hand
            var from = _Camera3D.ProjectRayOrigin(_GrabHand.GlobalPosition);
            var to = from + _Camera3D.ProjectRayNormal(_GrabHand.GlobalPosition) * 4444;
            _StockRaycast.GlobalPosition = from;
            _StockRaycast.TargetPosition = to;

            // If raycast is colliding, set that stock item to the hovered stock
            // item and activate its jiggle
            var collider = _StockRaycast.GetCollider();
            if (_StockRaycast.IsColliding() && collider is StockItem stockItem)
            {
                // If we already have one, but it's different, then unhover the
                // current one
                if (_HoveredStockItem != null && _HoveredStockItem != stockItem)
                {
                    _HoveredStockItem.Unhover();
                }

                // However, if we're already hovering it, ignore this
                if (_HoveredStockItem == stockItem) return;

                // Otherwise, set and hover
                _HoveredStockItem = stockItem;
                _HoveredStockItem.Hover();
            }
            // Otherwise, if we don't have a hovered stock item, unhover the
            // previous one
            else if (_HoveredStockItem != null)
            {
                _HoveredStockItem.Unhover();
                _HoveredStockItem = null;
            }
        }
    }
}
