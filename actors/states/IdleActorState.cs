using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Models;

namespace ShopIsDone.Actors.States
{
    public partial class IdleActorState : State
    {
        [Export]
        private ActorFloorIndicator _FloorIndicator;

        [Export]
        private NodePath _ModelPath;
        private IModel _Model;

        public override void _Ready()
        {
            _Model = GetNode<IModel>(_ModelPath);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Pull default animation out of message
            var anim = (string)message.GetValueOrDefault(Consts.ANIMATION_KEY, StateConsts.IDLE);

            // Hide floor indicator
            _FloorIndicator.Hide();

            // Idle state handler
            _Model.PerformAnimation(anim);

            // Base start
            base.OnStart(message);
        }
    }
}

