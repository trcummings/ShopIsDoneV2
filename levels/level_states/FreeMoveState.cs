using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Actors;
using ShopIsDone.Cameras;

namespace ShopIsDone.Levels.States
{
    public partial class FreeMoveState : State
    {
        [Export]
        private CameraService _CameraSystem;

        [Export]
        private PlayerCameraService _PlayerCameraService;

        [Export]
        private Haskell _Haskell;

        [Export]
        private InputXformer _InputXformer;
        private PlayerActorInput _PlayerInput;

        public override void _Ready()
        {
            base._Ready();
            _PlayerInput = new PlayerActorInput();
            _PlayerInput.Init(_InputXformer);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Activate camera system
            _CameraSystem.Init();
            _CameraSystem.SetCameraTarget(_Haskell).Execute();
            // Start actor
            _Haskell.Init(_PlayerInput);
            // Start camera
            _PlayerCameraService.Activate();

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

