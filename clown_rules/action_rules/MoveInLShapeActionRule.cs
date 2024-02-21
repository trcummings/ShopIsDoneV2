using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Extensions;
using System;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.ClownRules.ActionRules
{
    public partial class MoveInLShapeActionRule : ClownActionRule
    {
        public override bool BrokeRule(ArenaAction action, Dictionary<string, Variant> message)
        {
            return false;
            //// Unless this was a movement action, ignore it
            //var action = actionData.PawnAction;
            //if (!(action is WalkAction moveAction)) return new Command();

            //// If it is a movement action, but the moving pawn is not in a moving state,
            //// they haven't finished moving, so ignore it
            //if (moveAction.Pawn.GetComponent<StateHandlerComponent>().IsInState<MovingPawnState>()) return new Command();

            //// If it is, look through the move path and decide if they moved in an
            //// L-shaped path or not
            //var movePath = moveAction.GetMovePath(actionData.Message);
            //return new ConditionalCommand(
            //    () => MovedInLShape(movePath),
            //    BreakRule(actionData.Pawn)
            //);
        }

        protected bool MovedInLShape(Array<Tile> movePath)
        {
            // We moved in a L-shape if if the move vectors are two unbroken chunks
            // of the same vector and nothing else)
            var numChunks = movePath
                .WithPrevious(null)
                // Skip first where prev value is null
                .SkipWhile((record) => record.Previous == null)
                // Compare the values to get a diffed list of all movement
                .Select((record) => record.Previous.TilemapPosition - record.Current.TilemapPosition)
                // Make list nonconsecutive
                .NonConsecutive()
                // Get count
                .Count();

            // If num chunks is equal to 2, we moved in an L shape
            return numChunks == 2;
        }
    }
}