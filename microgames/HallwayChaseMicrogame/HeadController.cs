using System;
using Godot;

namespace ShopIsDone.Microgames.HallwayChase
{
	public partial class HeadController : Node3D
	{
		[Export]
		private Camera3D _Camera;

        [Export]
        private CharacterBody3D _Body;

        [Export]
        public float YLimit = 90f;

		[Export]
		public float BobFrequency = 5f;

        [Export]
        public float BobAmplitude = 0.1f;

		private float _BobTime;
        private float _YLimit;
		private Vector3 _Rotation;

		public override void _Ready()
		{
			_YLimit = Mathf.DegToRad(YLimit);
		}

        public void Look(Vector2 lookAxis, float lateralVelocity, float delta)
        {
			// Transform mouse look
			_Rotation.Y -= lookAxis.X;
			_Rotation.X = Mathf.Clamp(_Rotation.X - lookAxis.Y, -_YLimit, _YLimit);

			// Rotate Body on y-axis
			_Body.Rotation = new Vector3(_Body.Rotation.X, _Rotation.Y, _Body.Rotation.Z);
			// Rotate Head on x-axis, adding tilt to the z axis
			var moveTilt = Mathf.Lerp(_Rotation.Z, lateralVelocity, delta);
			Rotation = new Vector3(_Rotation.X, Rotation.Y, moveTilt);
        }

		public void ApplyHeadBob(bool isMoving, float percentOfMaxSpeed, float delta)
		{
			// If we're not on the floor or not moving, we're not bobbing
			if (!_Body.IsOnFloor() || !isMoving)
			{
				// Reset bob time
				_BobTime = 0;
				// Bring camera back to baseline
				_Camera.Position = _Camera.Position.Lerp(Vector3.Zero, delta);
            }
			// Otherwise, we're bobbing
			else
			{
				// Increment time
				_BobTime += delta;
				// Calculate bob
				var yBob = Mathf.Cos(_BobTime * BobFrequency * percentOfMaxSpeed) * BobAmplitude;
				// Interpolate camera to position
				var newPos = _Camera.Position;
				newPos.Y = yBob;
				_Camera.Position = _Camera.Position.Lerp(newPos, delta);
			}
		}
    }
}

