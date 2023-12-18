using Godot;
using System;

namespace Microgames.OnlyServiceAnimals
{
    public partial class Animal : Node2D
    {
        [Signal]
        public delegate void ReachedEndzoneEventHandler(AnimalTypes animalType);

        [Signal]
        public delegate void HitAttendantEventHandler(AnimalTypes animalType);

        public enum AnimalTypes
        {
            Normal,
            Service
        }

        [Export(PropertyHint.Enum, "Normal, Service")]
        public int AnimalType = 0;

        // Nodes
        private Area2D _EndzoneDetector;
        private Area2D _AttendantDetector;
        private AnimatedSprite2D _Sprite;
        private AnimationPlayer _AnimationPlayer;

        // State
        private Tween _MoveTween;

        public override void _Ready()
        {
            // Ready nodes
            _EndzoneDetector = GetNode<Area2D>("%EndzoneDetector");
            _AttendantDetector = GetNode<Area2D>("%AttendantDetector");
            _Sprite = GetNode<AnimatedSprite2D>("%Sprite");
            _AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

            // Connect
            _EndzoneDetector.Connect("area_entered", new Callable(this, nameof(OnEndzoneEntered)));
            _AttendantDetector.Connect("area_entered", new Callable(this, nameof(OnHitAttendant)));

            // Initially animate
            _Sprite.Play("run");
        }

        public void Start(Vector2 endpoint, float duration)
        {
            // Create the tween
            _MoveTween = GetTree().CreateTween();
            _MoveTween
                .TweenProperty(this, "global_position", endpoint, duration);
        }

        public void Stop()
        {
            // Remove the tween
            if (_MoveTween != null)
            {
                _MoveTween.Kill();
                _MoveTween = null;
            }
        }

        private void OnEndzoneEntered(Area2D _)
        {
            // Disable self
            _EndzoneDetector.SetDeferred("monitoring", false);
            _AttendantDetector.SetDeferred("monitoring", false);

            // Emit signal
            EmitSignal(nameof(ReachedEndzone), AnimalType);
        }

        private void OnHitAttendant(Area2D _)
        {
            // Stop self
            Stop();

            // Disable self
            _EndzoneDetector.SetDeferred("monitoring", false);
            _AttendantDetector.SetDeferred("monitoring", false);

            // Emit Signal
            EmitSignal(nameof(HitAttendant), AnimalType);

            // Spin off
            _Sprite.Play("splat");
            _AnimationPlayer.Play("SpinOff");
        }
    }
}