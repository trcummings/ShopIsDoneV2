using Godot;
using System;

namespace ShopIsDone.Microgames.HallwayChase
{
    // This is the class that controls the inner game
    public partial class HallwayChase : Node3D
    {
        [Signal]
        public delegate void PlayerReachedGoalEventHandler();

        [Signal]
        public delegate void PlayerFailedEventHandler();

        [Export]
        public AnimationPlayer _TargetAnimPlayer;

        private HallwayChasePlayer _Player;
        private AudioStreamPlayer _RoomTone;
        private Area3D _GoalArea;
        private Area3D _FailArea;

        public override void _Ready()
        {
            base._Ready();
            _Player = GetNode<HallwayChasePlayer>("%Player");
            _RoomTone = GetNode<AudioStreamPlayer>("%RoomTone");
            _GoalArea = GetNode<Area3D>("%GoalArea");
            _FailArea = GetNode<Area3D>("%FailArea");

            // Initially disable the player
            SetPlayerActive(false);

            // Connect to goal area and fail area
            _GoalArea.Connect("body_entered", new Callable(this, nameof(OnPlayerEnteredGoalArea)));
            _FailArea.Connect("body_entered", new Callable(this, nameof(OnPlayerEnteredFailArea)));
        }

        public void Init()
        {
            _TargetAnimPlayer.Play("Slump");
        }

        public void Start()
        {
            // Enable the player
            SetPlayerActive(true);

            // Fade in the room tone
            var goalDb = _RoomTone.VolumeDb;
            _RoomTone.VolumeDb = -80;
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_RoomTone, "volume_db", goalDb, 0.5f);
            _RoomTone.Play();
        }

        private void SetPlayerActive(bool value)
        {
            _Player.SetProcess(value);
            _Player.SetProcessInput(value);
            _Player.SetPhysicsProcess(value);
        }

        private void OnPlayerEnteredGoalArea(HallwayChasePlayer _)
        {
            SetPlayerActive(false);
            EmitSignal(nameof(PlayerReachedGoal));
        }

        private void OnPlayerEnteredFailArea(HallwayChasePlayer _)
        {
            SetPlayerActive(false);
            EmitSignal(nameof(PlayerFailed));
        }
    }
}

