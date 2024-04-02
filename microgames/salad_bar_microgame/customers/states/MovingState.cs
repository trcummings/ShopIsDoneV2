using System;
using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Microgames.SaladBar.States
{
    public partial class MovingState : State
    {
        // How long we want it to take to cross the screen
        [Export]
        public float MoveDuration = 3;

        [Export]
        private NodePath _CustomerPath;
        protected BaseCustomer _Customer;

        [Export]
        private AnimationPlayer _AnimationPlayer;

        [Export]
        private RayCast2D _CustomerDetector;

        // State
        protected Vector2 _StartPos;
        protected Vector2 _Destination;
        private bool _IsMoving = false;
        private Tween _MoveTween;
        private float _CumulativeTime = 0;

        public override void _Ready()
        {
            _Customer = GetNode<BaseCustomer>(_CustomerPath);
            SetPhysicsProcess(false);
        }

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            _StartPos = _Customer.GlobalPosition;
            _Destination = (Vector2)message?[BaseCustomer.DESTINATION_KEY];

            SetPhysicsProcess(true);

            StartMoving();
            base.OnStart(message);
        }

        public override void PhysicsUpdateState(double delta)
        {
            base.PhysicsUpdateState(delta);

            // If there's a customer in the way, stop moving
            if (_CustomerDetector.GetCollider() != null)
            {
                StopMoving();
            }
            // Otherwise, keep moving and accumulate movement time
            else
            {
                StartMoving();
                _CumulativeTime += (float)delta;
            }

            // If we're at the salad bar, stop all that stuff
            if (_Customer.GlobalPosition == _Destination)
            {
                if (_AnimationPlayer.HasAnimation("RESET")) _AnimationPlayer.Play("RESET");
                _Customer.EmitSignal(nameof(_Customer.ArrivedAtSaladBar));
                ChangeState("AtSaladBarState");
                return;
            }
        }

        private void StartMoving()
        {
            // Ignore if we're moving
            if (_IsMoving) return;

            // Restart the tween
            _MoveTween = GetTree().CreateTween().BindNode(this);
            _MoveTween.TweenProperty(_Customer, "global_position", _Destination, MoveDuration - _CumulativeTime);
            if (_AnimationPlayer.HasAnimation("walk")) _AnimationPlayer.Play("walk");
            _IsMoving = true;
        }

        private void StopMoving()
        {
            // Ignore if we're not moving
            if (!_IsMoving) return;

            // Kill the tween
            _MoveTween?.Kill();
            _MoveTween = null;
            if (_AnimationPlayer.HasAnimation("RESET")) _AnimationPlayer.Play("RESET");
            _IsMoving = false;
        }
    }
}

