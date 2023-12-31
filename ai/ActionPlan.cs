using System.Linq;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Pathfinding;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.AI
{
    public partial class ActionPlan : Resource
    {
        [Export]
        public string ActionId;

        protected LevelEntity _Entity;
        protected Dictionary<string, Variant> _Blackboard;
        protected ArenaAction _Action;

        [Inject]
        protected ActionService _ActionService;

        public void Init(ArenaAction action, Dictionary<string, Variant> blackboard)
        {
            _Action = action;
            _Entity = action.Entity;
            _Blackboard = blackboard;
        }

        public virtual bool IsValid()
        {
            // Just in case we weren't initialized properly
            if (_Action == null) return false;

            // Otherwise check if the action is available in the first place
            return _Action.IsAvailable();
        }

        public virtual int GetPriority()
        {
            return 1;
        }

        public virtual Command ExecuteAction()
        {
            // Execute the action without anything fancy otherwise
            return _ActionService.ExecuteAction(_Action);
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