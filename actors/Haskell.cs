using Godot;
using System;

namespace ShopIsDone.Actors
{
    public partial class Haskell : CharacterBody3D
    {
        [Export]
        private PlayerActorInput _ActorInput;

        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorFloorIndicator _ActorFloorIndicator;

        public void Init()
        {
            _ActorAnimator.Init();
        }

        public override void _Process(double _)
        {
            _ActorInput.UpdateInput();
            _ActorVelocity.AccelerateInDirection(_ActorInput.MoveDir);
            _ActorVelocity.Move(this);
            _ActorAnimator.UpdateAnimations(_ActorVelocity.Velocity);
            _ActorFloorIndicator.UpdateIndicator(_ActorVelocity.Velocity);
        }
    }
}
