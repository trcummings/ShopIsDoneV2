using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Extensions;

namespace ShopIsDone.Microgames.CheckInBack
{
    public partial class CheckInBackMicrogame : Microgame
    {
        [Export]
        public PackedScene RequestedItemScene;

        // Nodes
        private CharacterBody2D _Flashlight;
        private Area2D _ItemDetector;
        private Sprite2D _Reticule;

        // State
        private float _Speed;
        private float _Third;

        public override void _Ready()
        {
            // Base ready
            base._Ready();

            // Calculate speed (multiplied by one third per second)
            _Third = _Dim / 3f;
            _Speed = _Third * 1.5f * 60;
            // Ready nodes
            _Flashlight = GetNode<CharacterBody2D>("%Flashlight");
            _ItemDetector = GetNode<Area2D>("%ItemDetector");
            _Reticule = GetNode<Sprite2D>("%Reticule");

            // Get all placement points and process them into a map
            var allPoints = new Dictionary<Vector2, Node2D>();
            var candidates = new List<Vector2>();
            var points = GetTree().GetNodesInGroup("search_position").OfType<Node2D>();
            foreach (var point in points)
            {
                var nameArr = point.Name.ToString().Split(",");
                // Pull the placement position from the name
                var pos = new Vector2((int)GD.StrToVar(nameArr[0]), (int)GD.StrToVar(nameArr[1]));
                // Add to dictionary
                allPoints.Add(pos, point);
                candidates.Add(pos);
            }

            // Pick the placement point
            var chosenPoint = candidates.PickRandom();

            // Pick the flashlight placement points by eliminating adjacent choices
            // NB: Place the flashlight first because we don't want to risk an
            // accidental collision
            var flashlightPoint = candidates
                .Where(pos =>
                    // Not the chosen point
                    pos != chosenPoint &&
                    // No adjacent points
                    pos != chosenPoint + Vector2.Up &&
                    pos != chosenPoint + Vector2.Down &&
                    pos != chosenPoint + Vector2.Left &&
                    pos != chosenPoint + Vector2.Right
                )
                .ToList()
                .PickRandom();

            // Initially place flashlight
            _Flashlight.Position = new Vector2(
                // Position midway inside the horizontal third
                ((flashlightPoint.X - 1) * _Third) + (_Third / 2),
                // Position midway inside the vertical third (starting from bottom)
                _Dim - ((flashlightPoint.Y - 1) * _Third) - (_Third / 2)
            );

            // Initially place requested item
            var item = RequestedItemScene.Instantiate<Area2D>();
            var chosenPosition = allPoints[chosenPoint];
            chosenPosition.AddChild(item);

            // Connect to item detector signals
            _ItemDetector.Connect("area_entered", new Callable(this, nameof(OnItemEnteredDetector)));

            // Don't let player recieve input
            SetPhysicsProcess(false);
        }

        public override void Start()
        {
            base.Start();

            // Allow input
            SetPhysicsProcess(true);
        }

        protected override void OnTimerFinished()
        {
            // Stop all player input
            SetPhysicsProcess(false);
            // Fail sound
            PlayFailureSfx();
            // Emit
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        public override void _PhysicsProcess(double delta)
        {
            // Move flashlight
            var moveDir = Vector2.Zero;
            if (Input.IsActionPressed("move_up")) moveDir.Y -= 1;
            if (Input.IsActionPressed("move_down")) moveDir.Y += 1;
            if (Input.IsActionPressed("move_left")) moveDir.X -= 1;
            if (Input.IsActionPressed("move_right")) moveDir.X += 1;
            _Flashlight.Velocity = moveDir * _Speed * (float)delta;
            _Flashlight.MoveAndSlide();
        }

        private void OnItemEnteredDetector(Area2D _)
        {
            // Tween reticule alpha
            CreateTween()
                .BindNode(this)
                .TweenProperty(_Reticule, "modulate:a", 1f, 0.2f);
            // Stop timer
            MicrogameTimer.Stop();
            // Stop all player input
            SetPhysicsProcess(false);
            // Set outcome to win
            Outcome = Outcomes.Win;
            // Play victory noise
            PlaySuccessSfx();
            // Finish
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }
    }
}
