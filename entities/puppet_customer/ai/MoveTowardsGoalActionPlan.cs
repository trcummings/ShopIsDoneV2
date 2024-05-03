using Godot;
using System;
using ShopIsDone.AI;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Pathfinding;
using ShopIsDone.Actions;
using System.Linq;
using ShopIsDone.Widgets;
using ActionConsts = ShopIsDone.Actions.Consts;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
    public partial class MoveTowardsGoalActionPlan : ActionPlan
    {
        [Export]
        public int Distance = 1;

        private TileMovementHandler _MoveHandler;

        public override void Init(ArenaAction action, Dictionary<string, Variant> blackboard)
        {
            base.Init(action, blackboard);
            _MoveHandler = _Entity.GetComponent<TileMovementHandler>();
        }

        // If we're in bother range this is not a valid action, so get the
        // target, and then check the customer's tile neighbors for the target
        public override bool IsValid()
        {
            // Base valid check first
            if (!base.IsValid()) return false;

            // If blackboard doesn't have move target, this isn't valid
            if (
                !_Blackboard.ContainsKey(Consts.TILE_TARGET) ||
                !_Blackboard.ContainsKey(Consts.PRIORITY_GOAL_TILES)
            ) return false;

            var target = (Tile)_Blackboard[Consts.TILE_TARGET];
            return !TileWithinDistanceOfUs(target, Distance);
        }

        // Priority is as high as possible when we're not within the distance,
        // and as low as possible when we are
        public override int GetPriority()
        {
            var target = (Tile)_Blackboard[Consts.TILE_TARGET];
            return !TileWithinDistanceOfUs(target, Distance) ? int.MaxValue : int.MinValue;
        }

        public override Command ExecuteAction()
        {
            // Find a path towards the targeted tile
            var target = (Tile)_Blackboard[Consts.TILE_TARGET];
            var currentTile = GetTileAtTilemapPos(_Entity.TilemapPosition);

            // Get available moves
            var availableMoves = _MoveHandler.GetAvailableMoves(currentTile);

            // Get the target tiles to move to in order of preference
            var goalTiles = (Array<Tile>)_Blackboard[Consts.PRIORITY_GOAL_TILES];

            // Select a goal tile from our options
            var goalTile =
                // If there's a goal tile in our available moves, go with that one
                goalTiles.ToList().Find(availableMoves.Contains)
                // Otherwise, prioritise the highest value move
                ?? goalTiles.FirstOrDefault();

            // If no adjacent spaces are available, don't even move
            var finalPath = goalTile == null
                ? new Array<Tile>()
                : FindBestPath(currentTile, goalTile, availableMoves.ToGodotArray());

            return new SeriesCommand(
                // Show indicators if in light
                new ConditionalCommand(
                    currentTile.IsLit,
                    new ActionCommand(() => CreateTileIndicators(
                        availableMoves.Select(t => t.TilemapPosition),
                        TileIndicator.IndicatorColor.Blue
                    ))
                ),
                // Move
                _ActionService.ExecuteAction(_Action, new Dictionary<string, Variant>()
                {
                    { ActionConsts.MOVE_PATH, finalPath }
                }),
                // Hide indicators
                new ActionCommand(ClearTileIndicators)
            );
        }

        private Array<Tile> FindBestPath(Tile currentTile, Tile goalTile, Array<Tile> availableMoves)
        {
            // Otherwise, A* a good move path from the list of all tiles
            var moveFinder = new TileAStar();

            // Filter down all tiles to only theoretically possible moves
            // HACK: Use impossibly large pawn move range to 100000
            var allTheoreticalMoves = _MoveHandler.GetAvailableMoves(currentTile, true, 100000);

            // Find the best path to the destination
            var bestPath = moveFinder.GetMovePath(allTheoreticalMoves, currentTile, goalTile);

            // Truncate the best path by only allowing moves that are in our
            // available moves
            var movePath = bestPath.Where(availableMoves.Contains).ToList();

            // Prepend our initial position onto the move path
            movePath = movePath.Prepend(currentTile).ToList();

            return movePath.ToGodotArray();
        }
    }
}