using System;
using Godot;

namespace ShopIsDone.Actors
{
    public partial class ActorVelocity : Node
	{
		[Export]
		private float _MaxSpeed = 5;

		[Export]
		private float _AccelerationCoefficient = 1;
		public float AccelerationCoefficient { get { return _AccelerationCoefficient; } }

		public Vector3 Velocity { get; set; }

		public void AccelerateToVelocity(Vector3 velocity)
		{
			Velocity = Velocity.Lerp(velocity, 1f - Mathf.Exp(-_AccelerationCoefficient));
		}

		public void AccelerateInDirection(Vector3 dir)
		{
			AccelerateToVelocity(dir * _MaxSpeed);
		}

		public Vector3 GetMaxVelocity(Vector3 dir)
		{
			return dir * _MaxSpeed;
		}

		public void MaximizeVelocity(Vector3 dir)
		{
			Velocity = GetMaxVelocity(dir);
		}

		public void Decelerate()
		{
			AccelerateToVelocity(Vector3.Zero);
		}

		public void Move(CharacterBody3D body)
		{
			body.Velocity = Velocity;
			body.MoveAndSlide();
		}
	}
}

