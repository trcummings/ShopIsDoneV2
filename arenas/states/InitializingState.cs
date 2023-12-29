using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.States
{
	public partial class InitializingState : State
	{
        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);
            ChangeState("Running");
        }
    }
}
