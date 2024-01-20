using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Cameras
{
    // This is a service to be used to manually handle the camera via input
    public partial class PlayerCameraService : Node, IService
    {
        [Signal]
        public delegate void CameraRotatedEventHandler();

        [Export]
        private IsometricCamera _Camera;

        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
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

            if (Input.IsActionPressed("zoom_camera")) ZoomCameraIn();
            else ZoomCameraOut();
        }

        public void ZoomCameraIn()
        {
            _Camera.SetZoom(0.5f);
        }

        public void ZoomCameraOut()
        {
            _Camera.SetZoom(1);
        }

        public void Activate()
        {
            SetProcess(true);
        }

        public void Deactivate()
        {
            SetProcess(false);
        }
    }
}

