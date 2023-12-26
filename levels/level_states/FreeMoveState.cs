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
        [Signal]
        public delegate void CameraRotatedEventHandler();

        [Export]
        private CameraSystem _CameraSystem;

        [Export]
        private IsometricCamera _Camera;

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

            // Finish start hook
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Handle camera rotation
            if (Input.IsActionJustPressed("rotate_camera_left"))
            {
                _Camera.RotateLeft();
                EmitSignal(nameof(CameraRotated));
            }
            if (Input.IsActionJustPressed("rotate_camera_right"))
            {
                _Camera.RotateRight();
                EmitSignal(nameof(CameraRotated));
            }
        }
    }
}

