using Godot;
using ShopIsDone.Utils;
using System;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class Grabber : Node2D, ISlappable, IStoppable
    {
        [Signal]
        public delegate void GrabbedEventHandler();

        [Signal]
        public delegate void WithdrewEventHandler();

        [Signal]
        public delegate void SlappedEventHandler();

        protected AnimationPlayer _AnimPlayer;
        protected Area2D _Hitbox;

        [Export]
        public float GrabDelay = 1f;

        // State
        protected Tween _MoveTween;
        protected Vector2 _StartPoint;

        public override void _Ready()
        {
            base._Ready();
            _AnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
            _Hitbox = GetNode<Area2D>("%Hitbox");
        }

        public virtual void Start(Vector2 endpoint, float duration)
        {
            // Save the start point
            _StartPoint = GlobalPosition;

            // Create the tween
            _MoveTween = GetTree().CreateTween().BindNode(this);
            _MoveTween
                .TweenProperty(this, "global_position", endpoint, duration);

            // Connect to finished tween event
            _MoveTween.Connect(
                "finished",
                Callable.From(OnGrabberReachedTarget),
                (uint)ConnectFlags.OneShot
            );

            // Face destination
            LookAt(endpoint);
        }

        public virtual void Stop()
        {
            if (_MoveTween.IsRunning()) _MoveTween.Kill();
            _AnimPlayer.Stop();
        }

        public virtual void ReceiveSlap()
        {
            EmitSignal(nameof(Slapped));
            Withdraw();
        }

        protected virtual void OnGrabberReachedTarget()
        {
            // Wait and "threaten" to grab for a moment before grabbing
            GetTree().CreateTimer(GrabDelay).Connect(
                "timeout",
                Callable.From(OnGrabTimeout),
                (uint)ConnectFlags.OneShot
            );
        }

        private void OnGrabTimeout()
        {
            // If we're already withdrawing, ignore
            if (!_Hitbox.Monitorable) return;

            // Grab the target
            _AnimPlayer.Play("grab");
            // Emit grabbed event
            EmitSignal(nameof(Grabbed));
            // Withdraw
            Withdraw();
        }

        public virtual void Withdraw()
        {
            // If we're already withdrawing, ignore
            if (!_Hitbox.Monitorable) return;

            // Disable collider
            _Hitbox.Monitorable = false;

            // Play hurt animation
            if (_AnimPlayer.CurrentAnimation != "grab") _AnimPlayer.Play("hurt");

            // Tween Out
            _MoveTween.Kill();
            _MoveTween = GetTree().CreateTween().BindNode(this);
            _MoveTween
                .TweenProperty(this, "global_position", _StartPoint, 1f);
            _MoveTween.TweenCallback(Callable.From(() =>
            {
                EmitSignal(nameof(Withdrew));
                QueueFree();
            }));
        }
    }
}
