using Godot;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Tiles;
using ShopIsDone.EntityStates;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;
using SystemGeneric = System.Collections.Generic;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Actions
{
    public partial class MoveAction : ArenaAction
    {
        [Export]
        public MoveSubAction MoveSubAction;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ActionService _ActionService;

        private TileMovementHandler _MovementHandler;
        private EntityStateHandler _StateHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _MovementHandler = Entity.GetComponent<TileMovementHandler>();
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<TileMovementHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }

        // Visible in menu if we're in the idle state, as most actions are
        public override bool IsVisibleInMenu()
        {
            return _StateHandler.IsInState("idle");
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the move path from the message
            var movePath = (Array<Tile>)message[Consts.MOVE_PATH];
            var moveQueue = new SystemGeneric.Queue<Command>(_MovementHandler.GetMoveCommands(movePath.ToList()));

            return new SeriesCommand(
                // Add the base execution command (mark the action as completed)
                base.Execute(message),

                // Change the pawn to a movement state
                _StateHandler.RunChangeState("move"),

                // Accumulate the movement action as a series of conditional commands
                // that execute sub-movement actions which only continue if the unit
                // is still in a movement state after each one
                GenerateSubMovements(moveQueue),

                // At the end, if the pawn is still in a movement state, return it
                // to a normal state
                new ConditionalCommand(
                    () => _StateHandler.IsInState("move"),
                    _StateHandler.RunChangeState("idle")
                )
            );
        }

        protected Command GenerateSubMovements(SystemGeneric.Queue<Command> commands)
        {
            // Base case
            if (commands.Count == 0) return new Command();

            // Pluck next movement command from front of list
            var nextMovement = commands.Dequeue() as TileMovementHandler.PawnMoveCommand;

            // Create movement sub action
            var nextSubMove = (MoveSubAction)MoveSubAction.Duplicate();
            nextSubMove.Init(_ActionHandler);
            nextSubMove.NextMove = nextMovement;

            // Get direction towards
            var dirTowards = Entity.GetFacingDirTowards(nextMovement.FinalTile.TilemapPosition);

            // Induction step
            return new ConditionalCommand(
                () => _StateHandler.IsInState("move"),
                new SeriesCommand(
                    // Face towards the next tile
                    new ActionCommand(() => Entity.FacingDirection = dirTowards),
                    // Run deferrred calculation on if we should continue or fail the move
                    new DeferredCommand(() =>
                        new IfElseCommand(
                            // If the next tile is still available
                            () => !_TileManager.IsTileOccupied(nextMovement.FinalTile),
                            new SeriesCommand(
                                // Execute the sub-movement action with the given command
                                _ActionService.ExecuteAction(nextSubMove),
                                // Recurse
                                GenerateSubMovements(commands)
                            ),
                            // Otherwise, play interruption animation and change pawn back to a
                            // normal state
                            new SeriesCommand(
                                // Idle
                                _StateHandler.RunChangeState("idle"),
                                _StateHandler.RunPushState("alert")
                            )
                        )
                    )
                )
            );
        }
    }
}