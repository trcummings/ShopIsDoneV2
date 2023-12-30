using System.Linq;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Pathfinding;

namespace ShopIsDone.AI
{
    public partial class ActionPlan : Resource
    {
        protected LevelEntity _Entity;
        protected ActionHandler _ActionHandler;

        protected ArenaAction _Action;

        public void Init(ActionHandler actionHandler, ArenaAction action)
        {
            _ActionHandler = actionHandler;
            _Action = action;
        }

        public virtual bool IsValid()
        {
            // Otherwise check if the action is available in the first place
            return _ActionHandler.IsActionAvailable(_Action);
        }

        public virtual int GetPriority()
        {
            return 1;
        }

        public virtual Command ExecuteAction()
        {
            // Do nothing
            return new Command();
        }

        // Subclass sandbox methods
        protected bool InActionRange(TileManager tileManager, LevelEntity target)
        {
            // If the action range is 0, return false.
            if (_Action.Range == 0) return false;

            // Grab our current tile
            var currentTile = tileManager.GetTileAtTilemapPos(_Entity.TilemapPosition);
            // Grab the target tile
            var targetTile = tileManager.GetTileAtTilemapPos(target.TilemapPosition);

            // Get a path between the target and us
            var allTiles = tileManager.GetAllTilesInArena().ToList();
            var path = new TileAStar().GetMovePath(allTiles, currentTile, targetTile);

            // If the path length is equal to the action range, then we're in range
            return path.Count >= _Action.Range;
        }
    }
}