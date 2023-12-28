using Godot;
using System;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Widgets
{
    public partial class TileCursor : Node3D, IService
    {
        [Signal]
        public delegate void CursorEnteredTileEventHandler(Tile tile);

        [Signal]
        public delegate void CursorLeftTileEventHandler(Tile tile);

        [Signal]
        public delegate void AttemptedUnavailableMoveEventHandler();

        private TileManager _TileManager;

        // State
        public Tile CurrentTile = null;

        public void Init(TileManager tileManager)
        {
            _TileManager = tileManager;
        }

        public bool IsValidMove(Vector3 dir)
        {
            // Ignore if we're not yet on a tile or we dont have a tile manager
            if (CurrentTile == null || _TileManager == null) return false;

            // Attempt to move
            Tile newTile = _TileManager.GetTileAtTilemapPos(CurrentTile.TilemapPosition + dir);
            return newTile != null;
        }

        public void MoveCursorInDirection(Vector3 dir)
        {
            // Ignore if we're not yet on a tile or we dont have a tile manager
            if (CurrentTile == null || _TileManager == null) return;

            // Attempt to move to tile, so long as it exists, and is enabled
            Tile newTile = _TileManager.GetTileAtTilemapPos(CurrentTile.TilemapPosition + dir);
            if (newTile != null && newTile.Enabled) MoveCursorTo(newTile);
            else EmitSignal(nameof(AttemptedUnavailableMove));
        }

        public void MoveCursorTo(Tile tile)
        {
            var prevTile = CurrentTile;

            // Set current tile value
            CurrentTile = tile;

            // If it's a new tile, emit that we've left our previous one
            if (prevTile != null && prevTile != CurrentTile)
            {
                EmitSignal(nameof(CursorLeftTile), CurrentTile);
            }

            // Warp to above the given tile
            GlobalTransform = GlobalTransform with { Origin = CurrentTile.GlobalTransform.Origin };

            // Notify that we've entered the tile
            EmitSignal(nameof(CursorEnteredTile), CurrentTile);
        }
    }
}