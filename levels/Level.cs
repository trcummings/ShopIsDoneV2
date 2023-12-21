using Godot;
using System;
using ShopIsDone.Actors;
using ShopIsDone.Cameras;

namespace ShopIsDone.Levels
{
    public partial class Level : Node
    {
        [Export]
        private Node3D _DefaultSpawnPoint;

        [Export]
        private CameraSystem _CameraSystem;

        [Export]
        private IsometricCamera _Camera;

        [Export]
        private Haskell _Haskell;

        public override void _Ready()
        {
            SetProcess(false);
        }

        public void Init()
        {
            // Move actor to default spawn position
            _Haskell.GlobalTransform = _DefaultSpawnPoint.GlobalTransform;
            // Activate camera system
            _CameraSystem.Init();
            _CameraSystem.SetCameraTarget(_Haskell).Execute();
            // Start actor
            _Haskell.Init();
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("rotate_camera_left"))
            {
                _Camera.RotateLeft();
            }
            if (Input.IsActionJustPressed("rotate_camera_right"))
            {
                _Camera.RotateRight();
            }
        }
    }
}

