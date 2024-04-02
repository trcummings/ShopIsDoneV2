using Godot;
using System;
using System.Linq;
using Utils.Extensions;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class SaladBarArena : Node2D
    {
        [Signal]
        public delegate void HealthDamagedEventHandler(int amount);

        [Signal]
        public delegate void HealthDrainedEventHandler(int amount);

        [Export]
        public int HandSpeed = 10;

        [Export]
        public PackedScene NastyHandScene;

        [Export]
        public PackedScene TongsScene;

        [Export]
        public PackedScene ShamblerHandScene;

        [Export]
        public PackedScene FlyScene;

        [Export]
        public SaladBarEvents Events;

        private GlovedHand _GlovedHand;

        private Node2D _Grabbers;

        private Node2D _Flies;

        private Timer _FlyTimer;

        public Vector2 MoveInput;
        private RandomNumberGenerator _RNG = new RandomNumberGenerator();

        public override void _Ready()
        {
            _GlovedHand = GetNode<GlovedHand>("%GlovedHand");
            _Grabbers = GetNode<Node2D>("%Grabbers");
            _Flies = GetNode<Node2D>("%Flies");
            _FlyTimer = GetNode<Timer>("%FlyTimer");

            Events.NastyHandRequested += SpawnHand;
            Events.TongsRequested += SpawnTongs;
            Events.ShamblerHandRequested += SpawnShamblerHand;
        }

        public override void _Process(double _)
        {
            // Fist input
            if (Input.IsActionJustPressed("ui_accept")) _GlovedHand.Slap();
        }

        public override void _PhysicsProcess(double _)
        {
            // Get move input
            var moveDir = MoveInput;
            // Flip move dir in y direction
            moveDir.Y = -moveDir.Y;

            // Translate
            _GlovedHand.MoveHand(moveDir);

            // Clear move input
            MoveInput = Vector2.Zero;
        }

        public void TransitionToNight(float duration)
        {
            // Create tween
            var tween = GetTree().CreateTween();

            // Get all canvas modulate values and tween to darkness
            var darkness = new Color(0.05f, 0.05f, 0.05f, 1);
            var darkSources = GetTree()
                .GetNodesInGroup("salad_bar_darkness")
                .OfType<CanvasModulate>();
            foreach (var dark in darkSources)
            {
                dark.Color = Colors.White;
                dark.Show();
                tween
                    .BindNode(this)
                    .Parallel()
                    .TweenProperty(dark, "color", darkness, duration);
            }

            // Get all static lights and tween to light
            var staticLights = GetTree()
                .GetNodesInGroup("salad_bar_static_light")
                .OfType<Light2D>();
            foreach (var light in staticLights)
            { 
                light.Energy = 0;
                light.Show();
                tween
                    .Parallel()
                    .TweenProperty(light, "energy", 1, duration);
            }
        }

        public void ShowWeirdLights(float duration)
        {
            // Create tween
            var tween = GetTree().CreateTween();

            // Get all weird lights and tween to light
            var weirdLights = GetTree()
                .GetNodesInGroup("salad_bar_weird_light")
                .OfType<Light2D>();
            foreach (var light in weirdLights)
            {
                light.Energy = 0;
                light.Show();
                tween
                    .Parallel()
                    .TweenProperty(light, "energy", 1, duration);
            }
        }

        public void StartFlyTimer()
        {
            _FlyTimer.Start();
        }

        public void SetFlyTimer(float waitTime)
        {
            _FlyTimer.WaitTime = waitTime;
        }

        public void StopFlyTimer()
        {
            _FlyTimer.Stop();
        }

        public void SpawnFly()
        {
            // Create fly scene
            var fly = FlyScene.Instantiate<Fly>();
            // Connect to drain event
            fly.Drained += OnHealthDrained;
            // Pick random placement
            var _ = SetRandomPlacement(fly);
            var endPos = FindRandomQuadrantPoint();
            // Add to tree
            _Flies.AddChild(fly);
            // Let loose
            fly.Start(endPos, 1f);
        }

        public void SpawnHand()
        {
            // Create hand scene
            var hand = NastyHandScene.Instantiate<NastyHand>();
            // Pick random placement
            var goalPos = SetRandomPlacement(hand);
            // Connect to hand events
            hand.Grabbed += OnHandGrabbedVeggies;
            // Add to tree
            _Grabbers.AddChild(hand);
            // Let loose
            hand.Start(goalPos, 2.5f);
            // Emit
            Events.EmitSignal(nameof(Events.NastyHandSpawned), hand);
        }

        public void SpawnTongs()
        {
            // Create scene
            var tongs = TongsScene.Instantiate<Grabber>();
            // Pick random placement
            var goalPos = SetRandomPlacement(tongs);
            // Add to tree
            _Grabbers.AddChild(tongs);
            // Let loose
            tongs.Start(goalPos, 2.5f);
            // Emit
            Events.EmitSignal(nameof(Events.TongsSpawned), tongs);
        }

        public void SpawnShamblerHand()
        {
            // Create scene
            var shamblerHand = ShamblerHandScene.Instantiate<ShamblerHand>();
            // Pick random placement
            var goalPos = SetRandomPlacement(shamblerHand);
            // Connect to hand events
            shamblerHand.Drained += OnHealthDrained;
            // Add to tree
            _Grabbers.AddChild(shamblerHand);
            // Let loose
            shamblerHand.Start(goalPos, 1.5f);
            // Emit
            Events.EmitSignal(nameof(Events.ShamblerHandSpawned), shamblerHand);
        }

        private Vector2 SetRandomPlacement(Node2D node)
        {
            // Pick random placement
            var placement = GetRandomPlacement();
            var placementPos = placement.GetNode<Node2D>("Position2D");
            // Pick random angle from placement
            var deg = Mathf.RoundToInt(placement.RotationDegrees);
            var toolAngle = _RNG.RandiRange(-60, 60) + deg;
            var toolRads = Mathf.DegToRad(toolAngle);
            // Get new position vector
            var d = placement.GlobalPosition.DistanceTo(placementPos.GlobalPosition);
            var vec = new Vector2(Mathf.Cos(toolRads) * d, Mathf.Sin(toolRads) * d);
            // Position node
            node.GlobalPosition = vec + placement.GlobalPosition;

            // Return destination pos
            return placement.GlobalPosition;
        }

        private Vector2 FindRandomQuadrantPoint()
        {
            // Pick random placement
            var placement = GetRandomPlacement();
            // Get pivot's sibling area
            var collisionShape = placement
                .GetParent()
                .GetNode<Area2D>("Area2D")
                .GetNode<CollisionShape2D>("CollisionShape2D");

            // Grab shape from the collision
            var shape = collisionShape.Shape as RectangleShape2D;
            // Pick random point within extents (shrunken a little)
            var extents = shape.Size - new Vector2(10, 10);
            var randomPoint = new Vector2(
                _RNG.RandfRange(-extents.X, extents.X),
                _RNG.RandfRange(-extents.Y, extents.Y)
            );

            return placement.GlobalPosition + randomPoint;
        }

        private Node2D GetRandomPlacement()
        {
            // Pick random placement
            return GetTree()
                .GetNodesInGroup("saladbar_bin_pivot")
                .OfType<Node2D>()
                .ToList()
                .PickRandom();
        }

        public void SetHealth(int amount, int total)
        {
            var rottenBins = GetTree()
                .GetNodesInGroup("saladbar_rotten_bin")
                .OfType<Sprite2D>();
            var newModulate = Colors.Transparent;
            newModulate.A = (total - amount) / (float)total;
            foreach (var rottenBin in rottenBins) rottenBin.Modulate = newModulate;
        }

        private void OnHandGrabbedVeggies()
        {
            // Damage
            EmitSignal(nameof(HealthDamaged), 1);
        }

        private void OnHealthDrained(float amount)
        {
            // Drain
            EmitSignal(nameof(HealthDrained), amount);
        }
    }
}
