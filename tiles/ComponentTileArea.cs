using System;
using Godot;

namespace ShopIsDone.Tiles
{
    // This is a base class for various component areas that need to be detected,
    // as well as be associated with a specific tile underneath them
    public partial class ComponentTileArea : Area3D
    {
        [Export]
        private RayCast3D _TileRayCast;

        // The tile below this node
        public Tile Tile { get { return GetTile(); } }

        private Tile GetTile()
        {
            // Get collider from tile detector
            var collider = _TileRayCast.GetCollider();

            // If we're on a tile, return it
            if (collider is Tile tile) return tile;
            return null;
        }
    }
}

