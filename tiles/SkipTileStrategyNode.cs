using System;
using Godot;
using ShopIsDone.Utils.Pathfinding;

namespace ShopIsDone.Tiles
{
	public partial class SkipTileStrategyNode : Node, IShouldSkipTileStrategy
    {
        public virtual bool ShouldSkipTile(Tile currentCandidate, Tile currentNeighbor)
        {
            // Simple case
            return new SimpleShouldSkipTileStrategy().ShouldSkipTile(currentCandidate, currentNeighbor);
        }

        public virtual bool CanPassThroughUnitOnTile(Tile tile)
        {
            return true;
        }
    }
}

