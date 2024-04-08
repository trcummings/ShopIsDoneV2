using Godot;
using System;
using System.Linq;
using Godot.Collections;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class StockArea : Node3D, IDropzone
    {
        [Signal]
        public delegate void HandEnteredDropzoneEventHandler();

        [Signal]
        public delegate void HandExitedDropzoneEventHandler();

        public StockItem Item;
        private Area2D _Dropzone;

        private Node3D _StockItemsNode;
        private Marker3D _LeftPoint;
        private Marker3D _RightPoint;
        private Marker3D _BackPoint;
        private GodRayCubeHelper _GodRayCube;
        private Array<StockItem> _StockItems = new Array<StockItem>();

        public override void _Ready()
        {
            base._Ready();
            _LeftPoint = GetNode<Marker3D>("%LeftPoint");
            _RightPoint = GetNode<Marker3D>("%RightPoint");
            _BackPoint = GetNode<Marker3D>("%BackPoint");
            _GodRayCube = GetNode<GodRayCubeHelper>("%GodRayCube");
        }

        public bool CanDrop(StockItem item)
        {
            return _StockItems.Count <= 1 && item.Id == Item.Id;
        }

        public bool IsFull()
        {
            return _StockItems.Count == 2 && _StockItems.All(i => i.Id == Item.Id);
        }

        public void Init(Node3D itemsNode, Area2D area2D, StockItem item)
        {
            _StockItemsNode = itemsNode;
            _Dropzone = area2D;
            Item = item;

            // Connect to dropzone events
            area2D.BodyEntered += OnHandEntered;
            area2D.BodyExited += OnHandExited;

            foreach (var point in GetChildren().OfType<Marker3D>())
            {
                // Duplicate the item
                var newItem = item.Duplicate() as StockItem;

                // If it's the back point, disable the stock item collider and
                // add as our own child
                if (point == _BackPoint)
                {
                    newItem.SetDeferred("monitorable", false);
                    newItem.SetDeferred("monitoring", false);
                    newItem.SetCollisionLayerValue(2, false);
                    AddChild(newItem);
                }
                // Otherwise, add it as a child of the stock items
                else
                {
                    _StockItemsNode.AddChild(newItem);
                    _StockItems.Add(newItem);
                }

                // Position it
                newItem.Init(item.Id);
                newItem.GlobalPosition = point.GlobalPosition;
            }
        }

        public void DeleteAnItem()
        {
            // Pick random stock item to remove
            var randItem = _StockItems.PickRandom();
            _StockItems.Remove(randItem);
            randItem.QueueFree();
        }

        public void RemoveItem(StockItem item)
        {
            _StockItems.Remove(item);
        }

        public bool HasItem(StockItem item)
        {
            return _StockItems.Contains(item);
        }

        public void AddItem(StockItem item)
        {
            // Add item
            _StockItems.Add(item);
            item.GlobalPosition = FindEmptyPosition();
            item.RotateRandom();
        }

        public void Drop(StockItem item)
        {
            // Add item
            _StockItems.Add(item);

            // Tween item to new position
            var tween = GetTree()
                .CreateTween()
                .BindNode(this);
            tween.TweenProperty(item, "global_position", FindEmptyPosition(), 0.15f);
            tween.TweenCallback(Callable.From(item.RotateRandom));

        }

        public Vector3 FindEmptyPosition()
        {
            if (_StockItems.First().GlobalPosition.IsEqualApprox(_LeftPoint.GlobalPosition))
            {
                return _RightPoint.GlobalPosition;
            }
            else
            {
                return _LeftPoint.GlobalPosition;
            }
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
            if (node is GrabHand && !IsFull())
            {
                Hover();
                EmitSignal(nameof(HandEnteredDropzone));
            }
        }

        private void OnHandExited(Node2D node)
        {
            if (node is GrabHand)
            {
                Unhover();
                EmitSignal(nameof(HandExitedDropzone));
            }
        }
    }
}

