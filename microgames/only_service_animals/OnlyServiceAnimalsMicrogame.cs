using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using ShopIsDone.Cameras;
using static ShopIsDone.Cameras.ScreenshakeHandler;
using Utils.Extensions;

namespace ShopIsDone.Microgames.OnlyServiceAnimals
{
    public partial class OnlyServiceAnimalsMicrogame : Microgame
    {
        [Export]
        public PackedScene ServiceAnimalScene;

        [Export]
        public PackedScene NormalAnimalScene;

        // Nodes
        private Camera2D _Camera;
        private ScreenshakeHandler _Screenshake;
        private Node2D _ServiceAnimalIndicator;
        private Node2D _NormalAnimalIndicator;
        private Node2D _Animals;
        private AnimatedSprite2D _Attendant;
        private AudioStreamPlayer _ImpactSfxPlayer;

        // State
        private LanePoints _TopLane;
        private LanePoints _MiddleLane;
        private LanePoints _BottomLane;

        public struct LanePoints
        {
            public Vector2 StartPoint;
            public Vector2 AttendantPoint;
            public Vector2 EndPoint;
        }

        // Tweens
        private Tween _AttendantTween;
        private Vector2 _TargetAttendantPos = Vector2.Zero;

        public override void _Ready()
        {
            base._Ready();

            // Ready nodes
            _Camera = GetNode<Camera2D>("%Camera");
            _Screenshake = GetNode<ScreenshakeHandler>("%ScreenshakeHandler");
            _Attendant = GetNode<AnimatedSprite2D>("%Attendant");
            _ServiceAnimalIndicator = GetNode<Node2D>("%ServiceAnimalIndicator");
            _NormalAnimalIndicator = GetNode<Node2D>("%NormalAnimalIndicator");
            _Animals = GetNode<Node2D>("%Animals");
            _ImpactSfxPlayer = GetNode<AudioStreamPlayer>("%ImpactSfxPlayer");

            // Connect screenshake
            _Screenshake.Connect(nameof(ScreenshakeHandler.ShakeOffsetUpdated), new Callable(this, nameof(ShakeUpdate)));

            // Get all lane points
            _TopLane = CreateLanePoints(GetNode<Node2D>("%TopLane"));
            _MiddleLane = CreateLanePoints(GetNode<Node2D>("%MiddleLane"));
            _BottomLane = CreateLanePoints(GetNode<Node2D>("%BottomLane"));

            // Set target attendant pos
            _TargetAttendantPos = _Attendant.GlobalPosition;

            // Initialize indicators
            _ServiceAnimalIndicator.AddChild(ServiceAnimalScene.Instantiate<Animal>());
            _NormalAnimalIndicator.AddChild(NormalAnimalScene.Instantiate<Animal>());

            // Do not allow player input
            SetProcess(false);
        }

        public async override void Start()
        {
            base.Start();

            // Initially set outcome to win
            Outcome = Outcomes.Win;

            // Allow player input
            SetProcess(true);

            var lanePatterns = new List<List<LanePoints>>()
            {
                new List<LanePoints>() { _TopLane, _BottomLane, _MiddleLane },
                new List<LanePoints>() { _TopLane, _BottomLane, _TopLane },
                new List<LanePoints>() { _BottomLane, _MiddleLane, _BottomLane },
                new List<LanePoints>() { _BottomLane, _TopLane, _BottomLane },
            };

            var dogPatterns = new List<List<PackedScene>>()
            {
                new List<PackedScene>() { ServiceAnimalScene, ServiceAnimalScene, NormalAnimalScene },
                new List<PackedScene>() { ServiceAnimalScene, NormalAnimalScene, ServiceAnimalScene },
                new List<PackedScene>() { NormalAnimalScene, NormalAnimalScene, ServiceAnimalScene },
                new List<PackedScene>() { NormalAnimalScene, ServiceAnimalScene, NormalAnimalScene },
            };

            // Pick a random lane pattern
            var lanePattern = lanePatterns.PickRandom();
            // Pick a random dog pattern
            var dogPattern = dogPatterns.PickRandom();
            // Create waves with the pattern
            for (int i = 0; i < lanePatterns.Count - 1; i++)
            {
                // Get lane and dog
                var lane = lanePattern[i];
                var dog = dogPattern[i];

                // Spawn then wait
                SpawnAnimal(dog, lane.StartPoint, lane.EndPoint);
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            }
        }

        public override void _Process(double delta)
        {
            // Accumulate input for which track the attendant should be on
            var dir = 0;
            if (Input.IsActionPressed("move_up")) dir += 1;
            if (Input.IsActionPressed("move_down")) dir -= 1;

            // Handle input to get intended destination
            if (dir == 1) SetAttendantPoint(_TopLane.AttendantPoint);
            if (dir == 0) SetAttendantPoint(_MiddleLane.AttendantPoint);
            if (dir == -1) SetAttendantPoint(_BottomLane.AttendantPoint);
        }

        private void SpawnAnimal(PackedScene scene, Vector2 startPoint, Vector2 endPoint, float duration = 2f)
        {
            // Create node and add to animals node
            var animal = scene.Instantiate<Animal>();
            _Animals.AddChild(animal);
            animal.Hide();

            // Position it, set points, then show it
            animal.GlobalPosition = startPoint;
            animal.Show();

            // Connect to its events
            animal.Connect(nameof(Animal.HitAttendant), new Callable(this, nameof(OnAnimalHitAttendant)));
            animal.Connect(nameof(Animal.ReachedEndzone), new Callable(this, nameof(OnAnimalReachedEndzone)));

            // Start it
            animal.Start(endPoint, duration);
        }

        protected override void OnTimerFinished()
        {
            // Play success sound if we won
            if (Outcome == Outcomes.Win) PlaySuccessSfx();

            // Emit
            EmitSignal(nameof(MicrogameFinished), (int)Outcome);
        }

        private void OnAnimalHitAttendant(Animal.AnimalTypes animalType)
        {
            // Play hit SFX
            _ImpactSfxPlayer.Play();

            // If it's a service animal, shake and end the game
            if (animalType == Animal.AnimalTypes.Service)
            {
                // Mild Screenshake
                _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Mild)
                {
                    Axis = ShakeAxis.XOnly
                });

                // Fail the game
                FailGame();
            }
            // Otherwise, tiny shake
            else
            {
                _Screenshake.Shake(new ShakePayload(ShakePayload.ShakeSizes.Tiny)
                {
                    Axis = ShakeAxis.XOnly
                });
            }
        }

        private void OnAnimalReachedEndzone(Animal.AnimalTypes animalType)
        {
            // If it's a normal animal, end the game
            if (animalType == Animal.AnimalTypes.Normal) FailGame();
        }

        private void FailGame()
        {
            // Stop all player input
            SetProcess(false);

            // Pause all ongoing animals from moving
            foreach (var animal in _Animals.GetChildren().OfType<Animal>())
            {
                animal.Stop();
            }

            // Set outcome to loss
            Outcome = Outcomes.Loss;

            // Play failure sound
            PlayFailureSfx();

            // Finish early
            FinishEarly();
        }

        private void SetAttendantPoint(Vector2 targetPos)
        {
            // Ignore if no change
            if (targetPos == _TargetAttendantPos) return;

            // Update the target pos
            _TargetAttendantPos = targetPos;

            // If we have a tween, kill it
            if (_AttendantTween != null)
            {
                _AttendantTween.Kill();
                _AttendantTween = null;
            }

            // Tween the attendant to the target point
            _AttendantTween = GetTree().CreateTween().BindNode(this);
            _AttendantTween
                .TweenProperty(_Attendant, "global_position", _TargetAttendantPos, 0.05f)
                // Set ease and trans type
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Linear);
        }

        private LanePoints CreateLanePoints(Node2D laneNode)
        {
            return new LanePoints()
            {
                StartPoint = laneNode.GlobalPosition,
                AttendantPoint = laneNode.GetNode<Node2D>("AttendantPoint").GlobalPosition,
                EndPoint = laneNode.GetNode<Node2D>("EndPoint").GlobalPosition
            };
        }

        private void ShakeUpdate(Vector2 offset)
        {
            // These shake values are very small, so amplify them by a lot
            _Camera.Offset = offset * 1000;
        }
    }
}