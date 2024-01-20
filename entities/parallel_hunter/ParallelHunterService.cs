using System;
using Godot;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils;

namespace ShopIsDone.Entities.ParallelHunters
{
    public partial class ParallelHunterService : Node, IService, IInitializable
    {
        [Export]
        private bool _IsActive = false;

        [Export]
        private Array<LevelEntity> _Hunters = new Array<LevelEntity>();
        private Array<ParallelMoveHandler> _Movers = new Array<ParallelMoveHandler>();

        public void Init()
        {
            _Movers = _Hunters
                .Where(e => e.HasComponent<ParallelMoveHandler>())
                .Select(e => e.GetComponent<ParallelMoveHandler>())
                .ToGodotArray();
        }

        public Command RunParallelMoves(ArenaAction action, Command next)
        {
            // Wrap whole thing in a deferred command
            return new DeferredCommand(() =>
            {
                // Move on to the next action if this shouldn't run
                if (
                    // Is the service active?
                    !_IsActive ||
                    // If the action wasn't done by a player
                    action.Entity?.GetComponent<TeamHandler>()?.Team != TeamHandler.Teams.Player ||
                    // If it's not a movement sub action
                    action is not MoveSubAction subAction ||
                    // If the next actual movement is empty
                    subAction.NextMove == null
                ) return next;

                // Pull the next move out of the action
                var nextMove = subAction.NextMove;

                // Grab all the movers and order them by closeness to the target
                var sortedMovers = _Movers
                    .OrderBy(mover => mover.Entity.TilemapPosition.DistanceTo(action.Entity.TilemapPosition))
                    .ToGodotArray();
                foreach (var mover in sortedMovers)
                {
                    // Collect the movers that aren't this one
                    var otherMovers = sortedMovers
                        .Where(mc => mc.Entity != mover.Entity)
                        .ToGodotArray();

                    // Reserve this mover's next move
                    mover.ReserveNextParallelMove(action.Entity, nextMove, otherMovers);
                }

                // Execute the movements in parallel
                return new DeferredCommand(() => new ParallelCommand(
                    _Movers
                        .Select(m => m.ExecuteParallelAction())
                        // Append existing action to parallel queue
                        .Append(next)
                        .ToArray()
                ));
            });
        }
    }
}
