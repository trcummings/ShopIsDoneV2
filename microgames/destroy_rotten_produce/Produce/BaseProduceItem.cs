using Godot;
using System;

namespace Microgames.DestroyRottenProduce
{
    public partial class BaseProduceItem : RigidBody2D
    {
        [Export]
        public bool IsRotten = false;

        [Export]
        private AnimationPlayer _AnimPlayer;

        [Export]
        private CollisionShape2D _CollisionShape;

        [Export]
        private AnimatedSprite2D _AnimatedSprite;

        public bool WasHit { get { return _WasHit; } }
        private bool _WasHit = false;

        public async void TakeHit()
        {
            _WasHit = true;
            _CollisionShape.Disabled = true;

            _AnimPlayer.Play("SpinOff");
            await ToSignal(_AnimPlayer, "animation_finished");
            Hide();
        }

        public void Stop()
        {
            _AnimatedSprite.Stop();
        }
    }
}

