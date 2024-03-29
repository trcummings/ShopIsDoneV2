using Godot;
using ShopIsDone.Utils;
using System;

namespace ShopIsDone.Microgames.FindBathroom
{
    public partial class EmployeeCustomerPair : Node2D, IStoppable
    {
        [Signal]
        public delegate void EmployeeWasStruckEventHandler();

        [Signal]
        public delegate void CustomerWasStruckEventHandler();

        [Signal]
        public delegate void ReachedBathroomEventHandler();

        private Area2D _Employee;
        private Area2D _Customer;
        private AnimatedSprite2D _EmployeeSprite;
        private AnimatedSprite2D _CustomerSprite;
        private Area2D _BathroomDetector;

        // Positions
        private Node2D _LeftPoint;
        private Node2D _RightPoint;
        private Node2D _DownPoint;
        private Node2D _UpPoint;

        // State
        private Tween _PairTween;
        private Vector2 _FacingDir = Vector2.Left;

        public override void _Ready()
        {
            base._Ready();
            _BathroomDetector = GetNode<Area2D>("%BathroomDetector");
            _Employee = GetNode<Area2D>("%Employee");
            _EmployeeSprite = _Employee.GetNode<AnimatedSprite2D>("Sprite");
            _Customer = GetNode<Area2D>("%Customer");
            _CustomerSprite = _Customer.GetNode<AnimatedSprite2D>("Sprite");

            _LeftPoint = GetNode<Node2D>("%Left");
            _RightPoint = GetNode<Node2D>("%Right");
            _DownPoint = GetNode<Node2D>("%Down");
            _UpPoint = GetNode<Node2D>("%Up");

            // Initially walk
            _EmployeeSprite.Play("walk");
            _CustomerSprite.Play("walk");

            // Connect
            _Employee.AreaEntered += (Area2D _) =>
            {
                // Emit
                EmitSignal(nameof(EmployeeWasStruck));

                // TODO: Run animated sequence
            };
            _Customer.AreaEntered += (Area2D _) =>
            {
                // Emit
                EmitSignal(nameof(CustomerWasStruck));

                // TODO: Run animated sequence
            };
            _BathroomDetector.AreaEntered += (Area2D _) =>
            {
                // Emit
                EmitSignal(nameof(ReachedBathroom));
            };
        }

        public void MoveTo(Vector2 dir, Vector2 toPos)
        {
            // Create tween
            _PairTween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetParallel()
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Cubic);

            // Move to the new position
            _PairTween.TweenProperty(this, "global_position", toPos, 0.1f);

            // If we're moving in a new facing direction, tween the individual
            // pieces to their positions
            if (dir != _FacingDir)
            {
                var (employeePos, customerPos) = DirToPointPos(dir);

                _PairTween.TweenProperty(_Employee, "position", employeePos, 0.1f);
                _PairTween.TweenProperty(_Customer, "position", customerPos, 0.1f);
            }

            // Update facing dir
            _FacingDir = dir;
        }

        public override void _Process(double delta)
        {
            if (_FacingDir == Vector2.Right)
            {
                _EmployeeSprite.FlipH = true;
                _CustomerSprite.FlipH = true;
            }
            else if (_FacingDir == Vector2.Left)
            {
                _EmployeeSprite.FlipH = false;
                _CustomerSprite.FlipH = false;
            }
        }

        private (Vector2, Vector2) DirToPointPos(Vector2 dir)
        {
            if (dir == Vector2.Up) return (_UpPoint.Position, _DownPoint.Position);
            if (dir == Vector2.Down) return (_DownPoint.Position, _UpPoint.Position);
            if (dir == Vector2.Left) return (_LeftPoint.Position, _RightPoint.Position);
            if (dir == Vector2.Right) return (_RightPoint.Position, _LeftPoint.Position);
            return (Vector2.Zero, Vector2.Zero);
        }

        public bool IsRunning()
        {
            return _PairTween?.IsRunning() ?? false;
        }

        public void Stop()
        {
            _PairTween?.Kill();
            _PairTween = null;
        }
    }
}

