using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.Core;

namespace ShopIsDone.Tiles
{
    public partial class UnitTileMoveValidator : Node
    {
        [Export]
        private TileMovementHandler _TileMovementHandler;

        public bool IsValidMovePath(List<Tile> movePath)
        {
            // If not long enough, return false
            if (!IsMovePathLongEnough(movePath)) return false;

            // If the last position is unavailable, return false
            var lastTileInMove = movePath.Last();
            return CanLandOnTile(lastTileInMove);
        }

        protected virtual bool IsMovePathLongEnough(List<Tile> movePath)
        {
            // If we only have one tile (or less!) in our move set, false
            return movePath.Count > 1;
        }

        protected virtual bool CanLandOnTile(Tile tile)
        {
            // If the tile is in the light, test basic availability
            if (tile.IsLit() || _TileMovementHandler.CanSeeInTheDark)
            {
                return tile.IsTileAvailable();
            }

            // Otherwise, just assume that we can
            return true;
        }
    }
}

