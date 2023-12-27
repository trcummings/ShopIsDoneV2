using Godot;
using ShopIsDone.Utils;
using System;
using System.Linq;

namespace ShopIsDone.Cameras.Zooms
{
	public partial class OrthographicZoom : CameraZoom
    {
        // The base "size" zoom level that the camera starts with
        [Export]
        public float BaseZoom = 15f;

        // Zoom bounds
        [Export]
        public float MaxZoom = 25f;

        [Export]
        public float MinZoom = 5f;

        // The amount per frame the camera zooms in
        [Export]
        public float ZoomRate = 4f;

        // The amount per call that the zoom target gets set
        [Export]
        public float ZoomIncrement = 0.75f;

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
            var oldSize = _Camera.Size;
            _Camera.Size = (float)Mathf.Lerp(_Camera.Size, _TargetZoom, ZoomRate * delta);
            if (oldSize != _Camera.Size) EmitSignal(nameof(ZoomChanged), GetZoom());
        }

        public override void SyncClone(Camera3D clone)
        {
            if (!clone.IsInGroup("ignore_camera_clone_zoom")) clone.Size = _Camera.Size;
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
            return _Camera.Size / BaseZoom;
        }

        public override void SetZoom(float zoomAmount)
        {
            _TargetZoom = Mathf.Clamp(zoomAmount * BaseZoom, MinZoom, MaxZoom);
        }

        public override void ForceZoom(float zoomAmount)
        {
            // Forcibly set the camera zoom amount
            _Camera.Size = Mathf.Clamp(zoomAmount * BaseZoom, MinZoom, MaxZoom);
        }
	}
}