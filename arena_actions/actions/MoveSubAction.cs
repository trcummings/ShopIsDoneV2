using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Tiles;

namespace ShopIsDone.Actions
{
    // Note this is a one-of-a-kind "Sub Action" that is meant to be run as part
    // of the overall movement sequence. 
    public partial class MoveSubAction : ArenaAction
    {
        // Set on creation
        public TileMovementHandler.PawnMoveCommand NextMove;

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return NextMove;
        }
    }
}


