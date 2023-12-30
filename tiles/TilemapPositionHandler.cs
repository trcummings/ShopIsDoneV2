using Godot;
using ShopIsDone.Core;
using System;

namespace ShopIsDone.Tiles
{
    [Tool]
    public partial class TilemapPositionHandler : Node3DComponent
    {
        [Signal]
        public delegate void TilemapPositionChangedEventHandler(Vector3 pos);

        [Signal]
        public delegate void WentOffTilemapEventHandler();

        [Signal]
        public delegate void EnteredTilemapEventHandler();

        [Signal]
        public delegate void CurrentTileChangedEventHandler(Tile tile);

        [Export]
        private RayCast3D _TileDetector;

        [Export]
        public Vector3 TilemapPosition
        {
            get { return _TilemapPosition; }
            // Dummy set
            set { }
        }
        private Vector3 _TilemapPosition = Vector3.Inf;

        public Tile CurrentTile { get { return _CurrentTile; } }
        private Tile _CurrentTile = null;

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // Establish if we were off grid
            var wasOffGrid = TilemapPosition == Vector3.Inf;
            // Save current tile and pos
            var currentTile = _CurrentTile;

            // Get collider from tile detector
            var collider = _TileDetector?.GetCollider();
            // Check if it's a tile
            if (collider is Tile tile)
            {
                // If it's not the same tile, update the tile
                if (tile != currentTile)
                {
                    _CurrentTile = tile;
                    EmitSignal(nameof(CurrentTileChanged), _CurrentTile);

                    _TilemapPosition = tile.TilemapPosition;
                    EmitSignal(nameof(TilemapPositionChanged), _TilemapPosition);
                }

                // Emit if we were off grid, but are on grid now
                if (wasOffGrid) EmitSignal(nameof(EnteredTilemap));
            }
            // Otherwise, null out
            else
            {
                _TilemapPosition = Vector3.Inf;
                _CurrentTile = null;
                if (currentTile != null) EmitSignal(nameof(CurrentTileChanged), null);
                if (!wasOffGrid) EmitSignal(nameof(WentOffTilemap));
            }
        }
    }
}

