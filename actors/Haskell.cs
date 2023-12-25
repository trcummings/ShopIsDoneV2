using Godot;
using System;

namespace ShopIsDone.Actors
{
    public partial class Haskell : CharacterBody3D
    {
        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorFloorIndicator _ActorFloorIndicator;

        private IActorInput _ActorInput = new ActorInput();

        public void Init(IActorInput actorInput)
        {
            _ActorInput = actorInput;
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
