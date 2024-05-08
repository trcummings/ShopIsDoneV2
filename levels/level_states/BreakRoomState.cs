using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Utils;
using ShopIsDone.Actors;
using ShopIsDone.Cameras;

namespace ShopIsDone.Levels.States
{
    // When we enter a break room, we want to hide all followers of the leader,
    // and let the break room display the break room positions of the other
    // characters. In order to do this, we will idle the followers, hide them, and
    // then move them off screen
	public partial class BreakRoomState : State
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
            // Enable pausing
            _PauseInputHandler.IsActive = true;

            // Activate camera service
            _CameraService.Init();
            _CameraService.SetCameraTarget(_PlayerCharacterManager.Leader);
            // Start camera
            _PlayerCameraService.Activate();

            // Have leader move freely
            _PlayerCharacterManager.MoveFreely(_PlayerInput);
            // Idle the followers
            foreach (var follower in _PlayerCharacterManager.Followers) follower.Idle();
            // Place the followers in a far off position
            _PlayerCharacterManager.PlaceFollowers(Vec3.FarOffPoint, Vector3.Forward);

            // Finish the start hook
            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            // Deactivate camera service
            _PlayerCameraService.Deactivate();
            base.OnExit(nextState);
        }
    }
}