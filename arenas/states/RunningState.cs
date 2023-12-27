using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.States
{
	public partial class RunningState : State
	{
        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Consume phase commands as they come in

            // Start battle phase helper
        }
    }
}
