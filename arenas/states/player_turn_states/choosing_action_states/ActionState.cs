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
    }
}

