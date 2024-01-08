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

        // This function means that movement through this tile must happen as a
        // single consecutive movement because there is a unit on the given tile
        // we are able to pass through
        public virtual bool MustPassThroughTile(Tile tile)
        {
            return false;
        }
    }
}

