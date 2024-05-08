using Godot;
using System;
using ShopIsDone.AI;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Actions;
using System.Linq;
using ShopIsDone.Widgets;
using ActionConsts = ShopIsDone.Actions.Consts;
using ShopIsDone.Core;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
    public partial class FollowPathActionPlan : ActionPlan
    {
        private TileMovementHandler _MoveHandler;
        private ActionHandler _ActionHandler;

        public override void Init(ArenaAction action, Dictionary<string, Variant> blackboard)
        {
            base.Init(action, blackboard);
            _MoveHandler = _Entity.GetComponent<TileMovementHandler>();
            _ActionHandler = _Entity.GetComponent<ActionHandler>();
        }

        public override Command ExecuteAction()
        {
            // Find a path towards the targeted tile
            var pathTiles = (Array<Tile>)_Blackboard.GetValueOrDefault(Consts.FOLLOW_PATH_TILES, new Array<Tile>());
            var currentTile = GetTileAtTilemapPos(_Entity.TilemapPosition);

            // Take path values while there are no adjacent customer targets along the path
            var botherAction = _ActionHandler.GetAction(Consts.Actions.BOTHER_EMPLOYEE);

            // Check each way on the path, and if there's a target on any of the
            // tiles, break the loop
            var finalPath = new Array<Tile>();
            LevelEntity target = null;
            foreach (var tile in pathTiles)
            {
                var neighbors = tile.FindNeighbors().Values;
                foreach (var n in neighbors)
                {
                    if (botherAction.ContainsValidTarget(n))
                    {
                        target = n.UnitOnTile;
                        break;
                    }
                }

                finalPath.Add(tile);
                if (target != null) break;
            }

            // Get available moves
            var availableMoves = _MoveHandler.GetAvailableMoves(currentTile);

            return new SeriesCommand(
                new IfElseCommand(
                    () => target != null,
                    new ActionCommand(() => _Blackboard.SafeAdd(Consts.ENTITY_TARGET, target)),
                    new ActionCommand(() => _Blackboard.Remove(Consts.ENTITY_TARGET))
                ),
                // Show indicators
                new ActionCommand(() => CreateTileIndicators(
                    availableMoves.Select(t => t.TilemapPosition),
                    TileIndicator.IndicatorColor.Blue
                )),
                // Move
                _ActionService.ExecuteAction(_Action, new Dictionary<string, Variant>()
                {
                    { ActionConsts.MOVE_PATH, finalPath }
                }),
                // Hide indicators
                new ActionCommand(ClearTileIndicators)
            );
        }
    }
}