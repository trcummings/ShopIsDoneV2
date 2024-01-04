using System;
using Godot;
using Godot.Collections;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.StateMachine;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Actors.States
{
    public partial class InArenaActorState : State
    {
        [Export]
        private EntityStateHandler _StateHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            _StateHandler.ChangeState(StateConsts.IDLE);
            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            base.OnExit(nextState);
        }
    }
}

