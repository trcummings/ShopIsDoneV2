using Godot;
using ShopIsDone.Utils;
using System;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class Fly : Node2D, ISlappable, IStoppable
    {
        [Signal]
        public delegate void StartedEventHandler();

        [Signal]
        public delegate void LandedEventHandler();

        [Signal]
        public delegate void SlappedEventHandler();

        [Signal]
        public delegate void DrainedEventHandler(float amount);

        [Export]
        public float SecondsToDrain = 5;

        private AnimationPlayer _AnimPlayer;
        private Area2D _Hitbox;
        private Sprite2D _Puke;

        private float _DrainPerFrame;
        private bool _HasLanded = false;
        protected Tween _MoveTween;

        public override void _Ready()
        {
            _AnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
            _Hitbox = GetNode<Area2D>("%Hitbox");
            _Puke = GetNode<Sprite2D>("%Puke");
        }

        public virtual void Start(Vector2 endpoint, float duration)
        {
            // Calculate drain per frame
            _DrainPerFrame = 1 / (60 * SecondsToDrain);

            // Create the tween
            _MoveTween = GetTree().CreateTween().BindNode(this);
            // Create the movement tween
            _MoveTween
                .Parallel()
                .TweenProperty(this, "global_position", endpoint, duration);

            // Connect to finished tween event
            _MoveTween.Connect(
                "finished",
                Callable.From(OnFlyReachedTarget),
                (uint)ConnectFlags.OneShot
            );

            // Face destination
            LookAt(endpoint);

            EmitSignal(nameof(Started));
        }

        public void Stop()
        {
            if (_MoveTween.IsRunning()) _MoveTween.Kill();
            _AnimPlayer.Stop();
        }

        public void ReceiveSlap()
        {
            if (!_Hitbox.Monitorable) return;

            // Disable collision
            _Hitbox.Monitorable = false;
            // Emit signal
            EmitSignal(nameof(Slapped));
            // Kill movement tween
            _MoveTween.Kill();
            // Set variables
            _HasLanded = true;
            // Play splat animation
            _Puke.RotationDegrees = GD.Randi() % 360;
            _AnimPlayer.Play("splat");
            // Slowly fade away puke
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_Puke, "modulate:a", 0, 5f);
        }

        public override void _PhysicsProcess(double _)
        {
            if (_HasLanded && _Hitbox.Monitorable)
            {
                EmitSignal(nameof(Drained), _DrainPerFrame);
            }
        }

        private void OnFlyReachedTarget()
        {
            _HasLanded = true;
            EmitSignal(nameof(Landed));
        }
    }
}
