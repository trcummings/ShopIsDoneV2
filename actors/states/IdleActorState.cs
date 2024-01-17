using System;
using Godot;
using Godot.Collections;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.StateMachine;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Utils.Extensions;

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
            // Pull default animation out of message
            var anim = (string)message.GetValueOrDefault(Consts.ANIMATION_KEY, StateConsts.IDLE);

            // Hide floor indicator
            _FloorIndicator.Hide();

            // Idle state handler
            _StateHandler.ChangeState(anim);

            // Base start
            base.OnStart(message);
        }
    }
}

