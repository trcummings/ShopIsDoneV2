using Godot;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Tiles;
using ShopIsDone.EntityStates;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using SystemGeneric = System.Collections.Generic;
using StateConsts = ShopIsDone.EntityStates.Consts;

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
            return _StateHandler.IsInState(StateConsts.IDLE);
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
                _StateHandler.RunChangeState(StateConsts.MOVE),

                // Accumulate the movement action as a series of conditional commands
                // that execute sub-movement actions which only continue if the unit
                // is still in a movement state after each one
                GenerateSubMovements(moveQueue),

                // At the end, if the pawn is still in a movement state, return it
                // to an idle state
                new ConditionalCommand(
                    () => _StateHandler.IsInState(StateConsts.MOVE),
                    _StateHandler.RunChangeState(StateConsts.IDLE)
                )
            );
        }

        private Command GenerateSubMovements(SystemGeneric.Queue<Command> commands)
        {
            // Base case (return null sub move)
            if (commands.Count == 0) return _ActionService.ExecuteAction(NullSubMove());

            // Pluck next movement command from front of list
            var nextMovement = commands.Dequeue() as TileMovementHandler.PawnMoveCommand;

            // Create movement sub action
            var nextSubMove = (MoveSubAction)MoveSubAction.Duplicate();
            nextSubMove.Init(_ActionHandler);
            nextSubMove.NextMove = nextMovement;

            // Induction step
            return new ConditionalCommand(
                () => _StateHandler.IsInState(StateConsts.MOVE),
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
                            // Idle and run the alert state
                            _StateHandler.RunChangeState(StateConsts.IDLE),
                            _StateHandler.RunPushState(StateConsts.ALERT),
                            // Run the null sub move
                            _ActionService.ExecuteAction(NullSubMove())
                        )
                    )
                )
            );
        }

        private MoveSubAction NullSubMove()
        {
            var subMove = (MoveSubAction)MoveSubAction.Duplicate();
            subMove.Init(_ActionHandler);
            subMove.NextMove = null;
            return subMove;
        }
    }
}