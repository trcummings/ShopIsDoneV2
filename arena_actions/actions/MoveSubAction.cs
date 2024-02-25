using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Tiles;
using ShopIsDone.EntityStates;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Actions
{
    // Note this is a one-of-a-kind "Sub Action" that is meant to be run as part
    // of the overall movement sequence. 
    public partial class MoveSubAction : ArenaAction
    {
        // Set on creation
        public TileMovementHandler.PawnMoveCommand NextMove;

        private EntityStateHandler _StateHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // On last move, if the pawn is still in a movement state, return it
            // to an idle state
            return NextMove ?? (new ConditionalCommand(
                () => _StateHandler.IsInState(StateConsts.MOVE),
                _StateHandler.RunChangeState(StateConsts.IDLE)
            ) as Command);
        }

        public bool IsLastMove()
        {
            return NextMove == null;
        }
    }
}


