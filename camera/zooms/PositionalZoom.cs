using Godot;
using System;
using System.Linq;

namespace ShopIsDone.Cameras.Zooms
{
    // This is for handling "Positional Zoom", i.e moving the camera physically
    // closer. NB: This is basically useless for Ortographic projection
    public partial class PositionalZoom : CameraZoom
    {
        // The base zoom level that the camera starts with
        [Export]
        public float BaseZoom = 50f;

        // Zoom bounds
        [Export]
        public float MinZoom = 5f;

        [Export]
        public float MaxZoom = 100f;

        // The amount per frame the camera zooms in
        [Export]
        public float ZoomRate = 4.0F;

        // The amount per call that the zoom target gets set
        [Export]
        public float ZoomIncrement = 0.75F;

        // Zoom state
        private float _TargetZoom = 0f;

        public override void _Ready()
        {
            base._Ready();
            _TargetZoom = BaseZoom;
        }

        public override void UpdateZoom(double delta)
        {
            // Update zoom
            var oldSize = _Camera.Position.Z;
            _Camera.Position = _Camera.Position with
            {
                Z = (float)Mathf.Lerp(_Camera.Position.Z, _TargetZoom, ZoomRate * delta)
            };
            if (oldSize != _Camera.Position.Z) EmitSignal(nameof(ZoomChanged), GetZoom());
        }

        public override void SyncClone(Camera3D clone)
        {
            if (!clone.IsInGroup("ignore_camera_clone_zoom"))
            {
                clone.Position = clone.Position with { Z = _Camera.Position.Z };
            }
        }

        public override void ZoomIn()
        {
            _TargetZoom = Mathf.Max(_TargetZoom - ZoomIncrement, MinZoom);
        }

        public override void ZoomOut()
        {
            _TargetZoom = Mathf.Min(_TargetZoom + ZoomIncrement, MaxZoom);
        }

        public override float GetZoom()
        {
            return _Camera.Position.Z / BaseZoom;
        }

        public override void SetZoom(float zoomAmount)
        {
            _TargetZoom = Mathf.Clamp(zoomAmount * BaseZoom, MinZoom, MaxZoom);
        }

        public override void ForceZoom(float zoomAmount)
        {
            // Forcibly set the camera zoom amount
            _Camera.Position = _Camera.Position with
            {
                Z = Mathf.Clamp(zoomAmount * BaseZoom, MinZoom, MaxZoom)
            };
        }
    }
}

