using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Actors.States
{
	public partial class FollowLeaderActorState : State
	{
        [Export]
        private CharacterBody3D _Body;

        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorPathfinder _ActorPathfinder;

        private Node3D _Target;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Pull leader out of message
            _Target = (Node3D)message[Consts.LEADER_KEY];

            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Ignore if no target
            if (_Target == null) return;
            // Update path following
            _ActorPathfinder.SetTargetPosition(_Target.GlobalPosition);
            _ActorPathfinder.FollowPath();
            // Update the velocity component
            _ActorVelocity.Move(_Body);
            // Update animation
            _ActorAnimator.UpdateAnimations(_ActorVelocity.Velocity);
        }
    }
}
