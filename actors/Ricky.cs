using Godot;
using System;
using ShopIsDone.Core;

namespace ShopIsDone.Actors
{
    public partial class Ricky : LevelEntity
    {
        [Export]
        public Node3D Target;

        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorPathfinder _ActorPathfinder;

        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);
        }

        public override void Init()
        {
            _ActorAnimator.Init();
            SetProcess(true);
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
