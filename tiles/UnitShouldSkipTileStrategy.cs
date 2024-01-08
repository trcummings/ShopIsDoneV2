using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Actions;

namespace ShopIsDone.Tiles
{
	public partial class UnitShouldSkipTileStrategy : SkipTileStrategyNode
    {
        [Export]
        private TileMovementHandler _TileMoveHandler;

        [Export]
        private TeamHandler _TeamHandler;

        public override bool ShouldSkipTile(Tile currentCandidate, Tile currentNeighbor)
        {
            // If the next tile is too high up, ignore this tile
            if (IsTooHighUp(currentCandidate, currentNeighbor)) return true;

            // If we can't move through the tile, skip it
            if (!CanMoveThroughTile(currentCandidate, currentNeighbor)) return true;

            // Otherwise, simple case
            return base.ShouldSkipTile(currentCandidate, currentNeighbor);
        }

        public override bool MustPassThroughTile(Tile tile)
        {
            // If there's nothing on the tile, we don't need to "pass through"
            if (!tile.HasUnitOnTile()) return false;

            // If there's a unit on the tile and we're on the same team, we must
            // "pass through" it (though depending on the move handler's settings,
            // the given tile may not even be a candidate
            return _TeamHandler.IsOnSameTeam(tile.UnitOnTile);
        }

        protected virtual bool IsMovePathLongEnough(List<Tile> movePath)
        {
            // If we only have one tile (or less!) in our move set, false
            return movePath.Count > 1;
        }

        protected virtual bool IsTooHighUp(Tile currentCandidate, Tile currentNeighbor)
        {
            var heightDifference = Mathf.Abs(currentNeighbor.TilemapPosition.Y - currentCandidate.TilemapPosition.Y);
            return heightDifference > 1;
        }

        private bool CanPassThroughUnitOnTile(Tile tile)
        {
            // If no units on tile, we're clear
            if (tile.UnitOnTile == null) return true;

            // If there is, check if we're on the same team and that we can pass through allies
            return _TeamHandler.IsOnSameTeam(tile.UnitOnTile) && _TileMoveHandler.CanPassThroughAllies;
        }

        protected virtual bool CanMoveThroughTile(Tile currentCandidate, Tile currentNeighbor)
        {
            // If the tile is in the light, decide normally
            if (currentNeighbor.IsLit() || _TileMoveHandler.CanSeeInTheDark)
            {
                // Check for obstacles
                if (currentNeighbor.HasObstacleOnTile) return false;

                // Check for pass-through
                return CanPassThroughUnitOnTile(currentNeighbor);
            }

            // Otherwise, assume yes
            return true;
        }
    }
}

