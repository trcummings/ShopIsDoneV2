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

namespace ShopIsDone.Actions
{
    public partial class MoveAction : ArenaAction
    {
        [Export]
        public MoveSubAction MoveSubAction;

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ActionService _ActionService;

        private TileMovementHandler _MovementHandler;
        private EntityStateHandler _StateHandler;
        private FacingDirectionHandler _FacingDirectionHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _MovementHandler = Entity.GetComponent<TileMovementHandler>();
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
            _FacingDirectionHandler = Entity.GetComponent<FacingDirectionHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<FacingDirectionHandler>() &&
                entity.HasComponent<TileMovementHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the move path from the message
            var movePath = (Array<Tile>)message["MovePath"];
            var moveQueue = new SystemGeneric.Queue<Command>(_MovementHandler.GetMoveCommands(movePath.ToList()));

            return new SeriesCommand(
                // Add the base execution command (mark the action as completed)
                base.Execute(message),

                // Set the camera to follow the pawn
                _CameraService.SetCameraTarget(Entity),

                // Change the pawn to a movement state
                new AwaitSignalCommand(
                    _StateHandler,
                    nameof(_StateHandler.ChangedState),
                    nameof(_StateHandler.ChangeState),
                    "move"
                ),

                // Accumulate the movement action as a series of conditional commands
                // that execute sub-movement actions which only continue if the unit
                // is still in a movement state after each one
                GenerateSubMovements(moveQueue),

                // At the end, if the pawn is still in a movement state, return it
                // to a normal state
                new ConditionalCommand(
                    () => _StateHandler.IsInState("move"),
                    new AwaitSignalCommand(
                        _StateHandler,
                        nameof(_StateHandler.ChangedState),
                        nameof(_StateHandler.ChangeState),
                        "idle"
                    )
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
            var dirTowards = nextMovement.FinalTile.TilemapPosition - Entity.TilemapPosition;

            // Induction step
            return new ConditionalCommand(
                () => _StateHandler.IsInState("move"),
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
                            new AwaitSignalCommand(
                                _StateHandler,
                                nameof(_StateHandler.ChangedState),
                                nameof(_StateHandler.ChangeState),
                                "idle"
                            ),
                            // Face towards the interruption
                            new ActionCommand(() => _FacingDirectionHandler.FacingDirection = dirTowards),
                            // Push alert state
                            new AwaitSignalCommand(
                                _StateHandler,
                                nameof(_StateHandler.PushedState),
                                nameof(_StateHandler.PushState),
                                "alert"
                            )
                        )
                    )
                )
            );
        }
    }
}