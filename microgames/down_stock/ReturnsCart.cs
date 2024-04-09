using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class ReturnsCart : Node3D, IDropzone
    {
        [Signal]
        public delegate void DroppedItemInCartEventHandler(StockItem item);

        [Signal]
        public delegate void CartRattledEventHandler();

        public Marker3D DropzoneMarker { get { return _ShoppingCartMarker; } }
        private Marker3D _ShoppingCartMarker;

        public Marker3D GrabPlane { get { return _GrabPlane; } }
        private Marker3D _GrabPlane;

        private Marker3D _InCartPoint;

        private Area2D _Dropzone;
        private StockItem _Item;

        private GodRayCubeHelper _GodRayCube;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _GrabPlane = GetNode<Marker3D>("%GrabPlane");
            _ShoppingCartMarker = GetNode<Marker3D>("%ShoppingCartMarker");
            _GodRayCube = GetNode<GodRayCubeHelper>("%GodRayCube");
            _InCartPoint = GetNode<Marker3D>("%InCartPoint");
        }

        public void Init(Area2D area2D, StockItem item)
        {
            _Dropzone = area2D;
            _Item = item;

            // Connect to dropzone events
            area2D.BodyEntered += OnHandEntered;
            area2D.BodyExited += OnHandExited;
        }

        public bool CanDrop(StockItem _)
        {
            return true;
        }

        public void Drop(StockItem item)
        {
            // Emit
            EmitSignal(nameof(DroppedItemInCart), item);

            // Disable grab on item
            item.CanGrab = false;

            // Tween item into cart
            var tween = GetTree()
                .CreateTween()
                .BindNode(this);
            tween.TweenProperty(item, "global_position", _InCartPoint.GlobalPosition, 0.15f);
            tween.TweenCallback(Callable.From(() =>
            {
                // Sfx
                EmitSignal(nameof(CartRattled));

                // Wiggle
                var wiggleTween = GetTree()
                    .CreateTween()
                    .BindNode(this)
                    .SetTrans(Tween.TransitionType.Elastic)
                    .SetLoops(4);
                wiggleTween.TweenProperty(this, "position:x", 0.01f, .05f).AsRelative();
                wiggleTween.TweenProperty(this, "position:x", -0.01f, .05f).AsRelative();
            }));
        }

        public void Hover()
        {
            _GodRayCube.Reveal();
        }

        public void Unhover()
        {
            _GodRayCube.Collapse();
        }

        public bool IsDropzoneHovered()
        {
            return _Dropzone.GetOverlappingBodies().Any(b => b is GrabHand);
        }

        private void OnHandEntered(Node2D node)
        {
            if (node is GrabHand) Hover();
        }

        private void OnHandExited(Node2D node)
        {
            if (node is GrabHand) Unhover();
        }
    }
}

