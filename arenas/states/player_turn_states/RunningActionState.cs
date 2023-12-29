using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.PlayerTurn
{
	public partial class RunningActionState : State
	{
        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
        }
    }
}