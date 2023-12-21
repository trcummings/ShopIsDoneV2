using Godot;
using System;

namespace ShopIsDone.Cameras
{
    // This class is to help transform the input for the break room actor to
    // move around in a coherent manner regardless of camera rotation
    public partial class InputXformer : Node
    {
        [Export]
        private IsometricCamera _Camera;

        // These are the forward and right vectors for the camera
        private (Vector3, Vector3) _CameraVectors = (Vector3.Zero, Vector3.Zero);

        // Previous recorded input
        private Vector3 _PrevInput = Vector3.Zero;

        public void Init()
        {
            _CameraVectors = _Camera.GetCameraVectors();
        }

        public Vector3 GetXformedInput(Vector3 input)
        {
            // If there is a difference between the sign of the inputs between
            // this call and the last call, update the camera vectors
            if (Mathf.Sign(_PrevInput.X) != Mathf.Sign(input.X) || Mathf.Sign(_PrevInput.Z) != Mathf.Sign(input.Z))
            {
                _CameraVectors = _Camera.GetCameraVectors();
            }

            // Update prev input
            _PrevInput = input;

            // Return the xformed direction
            return IsometricCamera.GetXformedDir(_CameraVectors, new Vector2(input.X, input.Z), 0).Normalized();
        }
    }
}