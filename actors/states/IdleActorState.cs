using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Actors.States
{
    public partial class IdleActorState : State
    {
        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);
        }

        public override void OnExit(string nextState)
        {
            base.OnExit(nextState);
        }
    }
}

