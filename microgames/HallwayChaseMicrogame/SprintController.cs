using System;
using Godot;

namespace ShopIsDone.Microgames.HallwayChase
{
	public partial class SprintController : Node
	{
		[Export]
		private MovementController _Movement;

        [Export]
        private Camera3D _Camera;

		[Export]
		public float SprintSpeed = 16;

		[Export]
		public float FovMultiplier = 1.05f;

		private float _NormalSpeed;
        private float _NormalFov;

		public override void _Ready()
		{
			_NormalSpeed = _Movement.Speed;
			_NormalFov = _Camera.Fov;
		}

		public bool IsSprinting()
		{
			return _Movement.Speed == SprintSpeed;
		}

		public void Sprint(bool canSprint, float delta)
		{
			if (canSprint)
			{
				_Movement.Speed = SprintSpeed;
				_Camera.Fov = Mathf.Lerp(_Camera.Fov, _NormalFov * FovMultiplier, delta * 8);
			}
			else
			{
				_Movement.Speed = _NormalSpeed;
                _Camera.Fov = Mathf.Lerp(_Camera.Fov, _NormalFov, delta * 8);
            }
		}
    }
}

