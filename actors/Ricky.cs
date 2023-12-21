using Godot;
using System;

namespace ShopIsDone.Actors
{
    public partial class Ricky : CharacterBody3D
    {
        [Export]
        public Node3D Target;

        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorPathfinder _ActorPathfinder;

        public void Init()
        {
            _ActorAnimator.Init();
        }

        public override void _Process(double _)
        {
            // Update path following
            _ActorPathfinder.SetTargetPosition(Target.GlobalPosition);
            _ActorPathfinder.FollowPath();
            // Update the velocity component
            _ActorVelocity.Move(this);
            // Update animation
            _ActorAnimator.UpdateAnimations(_ActorVelocity.Velocity);
        }
    }
}
