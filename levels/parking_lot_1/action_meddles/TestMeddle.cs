using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Meddling;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System.Linq;
using ActionConsts = ShopIsDone.Actions.Consts;

namespace ShopIsDone.Levels.IntroLevel
{
    public partial class TestMeddle : ActionMeddle
    {
        [Export]
        private Node3D _FinalPoint;

        [Inject]
        private TileManager _TileManager;

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
                var destinationTile = _TileManager.GetTileAtGlobalPos(_FinalPoint.GlobalPosition);
                if (destinationTile != lastTile) return true;
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

