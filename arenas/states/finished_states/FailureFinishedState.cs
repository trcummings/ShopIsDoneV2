using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.States.Finished
{
	public partial class FailureFinishedState : State
	{
        [Export]
        private Arena _Arena;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
            // TODO: Failure screen
            _Arena.FailArena();
        }
    }
}