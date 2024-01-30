using System.Linq;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Pathfinding;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using static ShopIsDone.Widgets.TileIndicator;
using SystemGenerics = System.Collections.Generic;
using ShopIsDone.Widgets;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Lighting;

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

        [Inject]
        protected TileManager _TileManager;

        [Inject]
        protected TileIndicator _TileIndicator;

        private LightDetector _LightDetector;

        public virtual void Init(ArenaAction action, Dictionary<string, Variant> blackboard)
        {
            _Action = action;
            _Entity = action.Entity;
            _Blackboard = blackboard;

            if (_Entity.HasComponent<LightDetectorComponent>())
            {
                _LightDetector = _Entity.GetComponent<LightDetectorComponent>().Detector;
            }
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
            // Default to lowest priority
            return int.MinValue;
        }

        public virtual Command ExecuteAction()
        {
            return _ActionService.ExecuteAction(_Action);
        }

        // Subclass sandbox methods
        protected bool TileWithinDistanceOfUs(Tile target, int distance)
        {
            // If the distance is 0, return false.
            if (distance == 0) return false;

            // Grab our current tile
            var currentTile = GetTileAtTilemapPos(_Entity.TilemapPosition);

            // Get a path between the target and us
            var allTiles = GetAllTiles().ToList();
            var path = new TileAStar().GetMovePath(allTiles, currentTile, target);

            // If the path length is less than or equal to the distance, then
            // we're in range
            return path.Count <= distance;
        }

        protected Tile GetTileAtTilemapPos(Vector3 pos)
        {
            return _TileManager.GetTileAtTilemapPos(pos);
        }

        protected Array<Tile> GetAllTiles()
        {
            return _TileManager.GetAllTilesInArena();
        }

        protected void CreateTileIndicators(SystemGenerics.IEnumerable<Vector3> tiles, IndicatorColor color)
        {
            // Do not show any if we're not lit
            if (!IsLit()) return;

            // Filter out unlit tiles
            var allTiles = tiles
                .Select(_TileManager.GetTileAtTilemapPos)
                .Where(tile => tile.IsLit())
                .Select(tile => tile.GlobalPosition);

            _TileIndicator.CreateIndicators(allTiles, color);
        }

        protected void ClearTileIndicators()
        {
            _TileIndicator.ClearIndicators();
        }

        protected bool IsLit()
        {
            return _LightDetector?.IsLit() ?? true;
        }

        protected Command WaitFor(float timeout)
        {
            return new ConditionalCommand(
                // If pawn is not lit, don't wait, just skip forward
                IsLit,
                new WaitForCommand(_Entity, timeout)
            );
        }
    }
}