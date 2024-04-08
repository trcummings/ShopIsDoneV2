using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class ReturnsCart : Node3D, IDropzone
    {
        public Marker3D DropzoneMarker { get { return _ShoppingCartMarker; } }
        private Marker3D _ShoppingCartMarker;
        private Area2D _Dropzone;
        private StockItem _Item;

        private GodRayCubeHelper _GodRayCube;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _ShoppingCartMarker = GetNode<Marker3D>("%ShoppingCartMarker");
            _GodRayCube = GetNode<GodRayCubeHelper>("%GodRayCube");
        }

        public void Init(Area2D area2D, StockItem item)
        {
            _Dropzone = area2D;
            _Item = item;
        }

        public bool CanDrop(StockItem item)
        {
            return false;
        }

        public void Drop(StockItem item)
        {

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
            return false;
        }
    }
}

