using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class GlovedHand : CharacterBody2D
    {
        [Signal]
        public delegate void SlappedEventHandler();

        [Signal]
        public delegate void WhiffedEventHandler();

        [Export]
        public int HandSpeed = 7;

        private AnimationPlayer _AnimPlayer;
        private Area2D _Hurtbox;

        // State
        private bool _IsSlapping = false;

        public override void _Ready()
        {
            base._Ready();
            _AnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
            _Hurtbox = GetNode<Area2D>("%HurtBox");
        }

        public async void Slap()
        {
            // Ignore if currently slapping
            if (_IsSlapping) return;

            // Start slapping
            _IsSlapping = true;

            // Play slap
            _AnimPlayer.Play("slap");
            await ToSignal(_AnimPlayer, "animation_finished");
            _AnimPlayer.Play("RESET");

            // Stop slapping
            _IsSlapping = false;
        }

        public void MoveHand(Vector2 dir)
        {
            // Ignore if currently slapping
            if (_IsSlapping)
            {
                Velocity = Vector2.Zero;
                return;
            }

            // Translate
            Velocity = dir * HandSpeed * 100;
            MoveAndSlide();
        }

        private void CheckForSlapConnection()
        {
            // Make contact with slappable object if we have one, otherwise whiff
            var slappables = _Hurtbox
                .GetOverlappingAreas()
                .OfType<Area2D>()
                .Where(o => o.Owner is ISlappable)
                .Select(o => o.Owner);
            if (slappables.Any())
            {
                // Pick only the closest slappable
                var closest = slappables
                    .OrderBy(s => (s as Node2D).GlobalPosition.DistanceTo(GlobalPosition))
                    .First();
                EmitSignal(nameof(Slapped));
                (closest as ISlappable).ReceiveSlap();
            }
            // Otherwise whiff
            else EmitSignal(nameof(Whiffed));
        }
    }
}

