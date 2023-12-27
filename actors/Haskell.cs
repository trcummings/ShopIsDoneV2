using Godot;
using System;
using ShopIsDone.Core;

namespace ShopIsDone.Actors
{
    public partial class Haskell : LevelEntity
    {
        [Export]
        private ActorAnimator _ActorAnimator;

        [Export]
        private ActorVelocity _ActorVelocity;

        [Export]
        private ActorFloorIndicator _ActorFloorIndicator;

        private IActorInput _ActorInput = new ActorInput();

        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);
        }

        public void Init(IActorInput actorInput)
        {
            _ActorInput = actorInput;
            _ActorAnimator.Init();
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            _ActorInput.UpdateInput();
            _ActorVelocity.AccelerateInDirection(_ActorInput.MoveDir);
            _ActorVelocity.Move(this);
            _ActorAnimator.UpdateAnimations(_ActorVelocity.Velocity);
            _ActorFloorIndicator.UpdateIndicator(_ActorVelocity.Velocity);
        }
    }
}
