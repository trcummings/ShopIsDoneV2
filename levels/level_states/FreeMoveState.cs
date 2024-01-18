using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Actors;
using ShopIsDone.Cameras;
using ShopIsDone.Pausing;

namespace ShopIsDone.Levels.States
{
    public partial class FreeMoveState : State
    {
        [Export]
        private CameraService _CameraService;

        [Export]
        private PlayerCameraService _PlayerCameraService;

        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private InputXformer _InputXformer;
        private PlayerActorInput _PlayerInput;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        public override void _Ready()
        {
            base._Ready();
            _PlayerInput = new PlayerActorInput();
            _PlayerInput.Init(_InputXformer);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Activate camera service
            _CameraService.Init();
            _CameraService.SetCameraTarget(_PlayerCharacterManager.Leader).Execute();
            // Allow characters to move freely
            _PlayerCharacterManager.MoveFreely(_PlayerInput);
            // Place the followers back near the leader
            var leader = _PlayerCharacterManager.Leader;
            _PlayerCharacterManager.PlaceFollowers(leader.GlobalPosition, leader.FacingDirection);

            // Start camera
            _PlayerCameraService.Activate();

            // Allow pausing
            _PauseInputHandler.IsActive = true;

            // Finish start hook
            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            _PlayerCameraService.Deactivate();
            base.OnExit(nextState);
        }
    }
}

