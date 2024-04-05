using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class DraggingDownStockState : DownStockState
    {
        [Export]
        private Marker3D _GrabPlane; // Just used for its Z value

        [Export]
        private Camera3D _Camera3D;

        private StockItem _GrabbedStockItem = null;
        private Transform3D _StockItemInitialTransform;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Pull the stock item from the message
            _GrabbedStockItem = (StockItem)message[Consts.HOVERED_ITEM_KEY];
            // Persist its initial position
            _StockItemInitialTransform = _GrabbedStockItem.Transform;
            // Wiggle it
            _GrabbedStockItem.Wiggle();

            // Have hand grab
            _GrabHand.Grab();

            // Tween the stock item to the grab plane
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_GrabbedStockItem, "global_position:z", _GrabPlane.GlobalPosition.Z, 0.15f);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            if (Input.IsActionJustReleased("ui_accept"))
            {
                // If we're over a dropzone, release it there

                // Otherwise, let it fly back to where it was
                GetTree()
                    .CreateTween()
                    .BindNode(this)
                    .TweenProperty(_GrabbedStockItem, "transform", _StockItemInitialTransform, 0.15f);
                // Change back to hovering state
                ChangeState(Consts.States.HOVERING);
            }
        }

        public override void PhysicsUpdateState(double delta)
        {
            base.PhysicsUpdateState(delta);

            // Update hand pos
            UpdateHandPosition((float)delta);

            // Move grabbed stock item in line with the grab hand cursor
            var from = _Camera3D.ProjectRayOrigin(_GrabHand.GlobalPosition);
            var dist = _Camera3D.GlobalPosition.Z - _GrabPlane.GlobalPosition.Z;
            var to = from + _Camera3D.ProjectRayNormal(_GrabHand.GlobalPosition) * dist;
            _GrabbedStockItem.GlobalPosition = _GrabbedStockItem
                .GlobalPosition
                .MoveToward(to, (float)delta * 4);
        }
    }
}
