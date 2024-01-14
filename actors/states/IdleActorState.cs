using System;
using Godot;
using Godot.Collections;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.StateMachine;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Actors.States
{
    public partial class IdleActorState : State
    {
        [Export]
        private EntityStateHandler _StateHandler;

        [Export]
        private ActorFloorIndicator _FloorIndicator;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Hide floor indicator
            _FloorIndicator.Hide();

            // Idle state handler
            _StateHandler.ChangeState(StateConsts.IDLE);

            // Base start
            base.OnStart(message);
        }
    }
}

