using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Meddling;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using System;
using System.Linq;
using ActionConsts = ShopIsDone.Actions.Consts;

namespace ShopIsDone.Levels.IntroLevel
{
    public partial class TestMeddle : ActionMeddle
    {
        [Export]
        private Node3D _FinalPoint;

        public override bool ShouldMeddle(ArenaAction action, Dictionary<string, Variant> message)
        {
            // If we move anywhere other than the light switch, this should stop us
            if (action is MoveAction)
            {
                // Grab move path from the action
                var movePath = (Array<Tile>)message[ActionConsts.MOVE_PATH];
                // Get the last tile from the move path
                var lastTile = movePath.Last();
                // If the last tile of the move path isn't equal to our designated final point,
                // reject the action
                if (!_FinalPoint.GlobalPosition.IsEqualApprox(lastTile.GlobalPosition)) return true;
                // Otherwise it's valid
                return false;
            }
            // Do not meddle in sub movements
            else if (action is MoveSubAction)
            {
                return false;
            }
            // If it's any other type of action, we should meddle in it
            return true;
        }

        public override Command Meddle(ArenaAction action, Dictionary<string, Variant> message)
        {
            return new ActionCommand(() =>
            {
                GD.Print("Action Denied");
            });
        }
    }
}

