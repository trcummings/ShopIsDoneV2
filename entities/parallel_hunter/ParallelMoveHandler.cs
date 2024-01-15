using System;
using System.Linq;
using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Collections.Generic;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Pathfinding;

namespace ShopIsDone.Entities.ParallelHunters
{
    public partial class ParallelMoveHandler : NodeComponent
    {
        [Export]
        private TileMovementHandler _MoveHandler;

        public Tile ChaseTo { get; private set; }

        [Inject]
        private TileManager _TileManager;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public void ReserveNextParallelMove(
            LevelEntity target,
            TileMovementHandler.PawnMoveCommand targetAction,
            Array<ParallelMoveHandler> otherMovers
        )
        {
            // Initially null out our ChaseTo
            ChaseTo = null;

            // Pull movement out of target action
            // We are aiming to end up right behind the target, but barring
            // that we'll take anything that isn't foreclosed on or where
            // the target is currently moving to
            var targetToTile = targetAction.FinalTile;

            // Construct set of foreclosed moves
            var foreclosedMoves = otherMovers.Aggregate(new HashSet<Tile>(), (acc, mover) => {
                // If the other mover has already reserved this tile, add it
                // to the foreclosed moves
                if (mover.ChaseTo != null) acc.Add(mover.ChaseTo);
                return acc;
            });

            // Get current tile
            var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);

            // Find closest available tile that is available, and not already
            // reserved
            var availableMoves = _MoveHandler
                // HACK: Use impossibly large pawn move range to 100000
                .GetAvailableMoves(currentTile, true, 100000)
                // Filter down by already foreclosed moves and target's move to tile
                .Where(t => t != targetToTile && !foreclosedMoves.Contains(t))
                // Order by closeness to target
                .OrderBy(t => t.TilemapPosition.DistanceTo(target.TilemapPosition))
                .ToList();

            // If no closest move exists, don't move
            var closestMove = availableMoves.FirstOrDefault();
            if (closestMove == null) return;

            // Get best path to the closest move
            var bestPath = new TileAStar().GetMovePath(availableMoves, currentTile, closestMove);

            // If best path is empty, don't move
            if (bestPath.Count == 0) return;

            // Grab the first move from the best path and reserve it
            var firstBestMove = bestPath.First();
            ChaseTo = firstBestMove;
        }

        public Command ExecuteParallelAction()
        {
            return new DeferredCommand(() =>
            {
                // Null command if no chase to found
                if (ChaseTo == null) return new Command();

                // Get current tile
                var currentTile = _TileManager.GetTileAtTilemapPos(Entity.TilemapPosition);

                // Create move path
                var movePath = new Array<Tile>() { currentTile, ChaseTo };

                // Get move commands
                return new SeriesCommand(
                    _MoveHandler.GetMoveCommands(movePath.ToList()).ToArray()
                );
            });
        }
    }
}

