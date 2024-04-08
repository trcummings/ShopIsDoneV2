using System;
using System.Linq;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class DraggingDownStockState : DownStockState
    {
        [Signal]
        public delegate void AttemptedIncorrectDropEventHandler();

        [Export]
        private Marker3D _GrabPlane; // Just used for its Z value

        [Export]
        private Camera3D _Camera3D;

        [Export]
        private Node3D _StockAreasNode;
        private Array<StockArea> _StockAreas = new Array<StockArea>();

        // State
        private StockItem _GrabbedStockItem = null;
        private Transform3D _StockItemInitialTransform;
        private IDropzone _CurrentHovered = null;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Pull the stock item from the message
            _GrabbedStockItem = (StockItem)message[Consts.HOVERED_ITEM_KEY];
            // Persist its initial position
            _StockItemInitialTransform = _GrabbedStockItem.Transform;
            // Wiggle it
            _GrabbedStockItem.Wiggle();

            // Collect all stock areas
            _StockAreas = _StockAreasNode
                .GetChildren()
                .OfType<StockArea>()
                .ToGodotArray();

            // Have hand grab
            _GrabHand.Grab();
            _GrabbedStockItem.Grab();

            // Tween the stock item to the grab plane
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_GrabbedStockItem, "global_position:z", _GrabPlane.GlobalPosition.Z, 0.15f);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Handle dropzone hovering and unhovering 
            var hoveredDropzones = _StockAreas.Where(a => a.IsDropzoneHovered());
            if (hoveredDropzones.Any() && hoveredDropzones.First() != _CurrentHovered)
            {
                // Set current hovered
                _CurrentHovered = hoveredDropzones.First();
            }
            // Handle when we have no hovered dropzones
            else if (!hoveredDropzones.Any())
            {
                // Null out and unhover
                _CurrentHovered = null;
            }

            if (Input.IsActionJustReleased("ui_accept"))
            {
                // If we're over a dropzone, release it there
                if (_CurrentHovered?.CanDrop(_GrabbedStockItem) ?? false)
                {
                    // Remove from previous stock area
                    _StockAreas.ToList().Find(a => a.HasItem(_GrabbedStockItem))?.RemoveItem(_GrabbedStockItem);
                    // Drop into the new one
                    _CurrentHovered.Drop(_GrabbedStockItem);
                }
                // Otherwise, let it fly back to where it was
                else if (_CurrentHovered != null)
                {
                    EmitSignal(nameof(AttemptedIncorrectDrop));
                    ReturnSelected();
                }
                else ReturnSelected();

                // Ungrab for the sfx
                _GrabbedStockItem.Grab();
                // Either way, change back to hovering state
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

        private void ReturnSelected()
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_GrabbedStockItem, "transform", _StockItemInitialTransform, 0.15f);
        }
    }
}
