using Godot;
using ShopIsDone.Actions;
using System;
using Godot.Collections;

namespace ShopIsDone.ClownRules.ClownActionRules
{
    public partial class ClownActionRule : Node
    {
        // Cosmetics
        [Export(PropertyHint.MultilineText)]
        public string RuleDescription = "";

        public virtual bool BrokeRule(ArenaAction action, Dictionary<string, Variant> message)
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
    }
}

