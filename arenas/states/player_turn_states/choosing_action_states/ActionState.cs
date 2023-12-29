using System;
using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    // Base class for the choosing action sub state machine
	public partial class ActionState : State
    {
        [Signal]
        public delegate void GoBackRequestedEventHandler();

        [Signal]
        public delegate void MainMenuRequestedEventHandler();

        [Signal]
        public delegate void RunActionRequestedEventHandler(Dictionary<string, Variant> message);

        [Signal]
        public delegate void PawnAPDiffRequestedEventHandler(ActionPointHandler apData);

        [Signal]
        public delegate void PawnAPDiffCanceledEventHandler();


        protected void RequestApDiff(ActionPointHandler apHandler, ActionPointHandler subtractableAp)
        {
            EmitSignal(nameof(PawnAPDiffRequested), new ActionPointHandler()
            {
                ActionPoints = Mathf.Max(apHandler.ActionPoints - subtractableAp.ActionPoints, 0),
                ActionPointDebt = Mathf.Max(apHandler.ActionPointDebt - subtractableAp.ActionPointDebt, 0),
                ActionPointExcess = Mathf.Max(apHandler.ActionPointExcess - subtractableAp.ActionPointExcess, 0)
            });
        }

        protected void CancelApDiff()
        {
            EmitSignal(nameof(PawnAPDiffCanceled));
        }
    }
}

