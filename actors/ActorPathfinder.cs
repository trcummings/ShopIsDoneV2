using System;
using Godot;

namespace ShopIsDone.Actors
{
	public partial class ActorPathfinder : Node
	{
        [Export]
        private Node3D _Body;

        [Export]
		public NavigationAgent3D NavigationAgent;

        // Velocity component goes here
        [Export]
        private ActorVelocity _VelocityComponent;

		// Interval timer to update target position
        [Export]
		private Timer _IntervalTimer;

		public override void _Ready()
		{
			NavigationAgent.Connect("velocity_computed", new Callable(this, nameof(OnVelocityComputed)));
		}

		public void FollowPath()
		{
			if (NavigationAgent.IsNavigationFinished())
			{
				_VelocityComponent.Decelerate();
				return;
			}

			// Otherwise, pick new direction
			var dir = (NavigationAgent.GetNextPathPosition() - _Body.GlobalPosition).Normalized();
			// Accelerate velocity component in that direction
			_VelocityComponent.AccelerateInDirection(dir);
            // Set the nav agent's velocity from the velocity component
            NavigationAgent.Velocity = _VelocityComponent.Velocity;
		}

		public void SetTargetPosition(Vector3 targetPosition)
		{
			if (!_IntervalTimer.IsStopped()) return;
			_IntervalTimer.Start();
			NavigationAgent.TargetPosition = targetPosition;
		}

		public void ForceSetTargetPosition(Vector3 targetPosition)
		{
            NavigationAgent.TargetPosition = targetPosition;
            _IntervalTimer.Start();
        }

		private void OnVelocityComputed(Vector3 safeVelocity)
		{
			// Get normalized velocity from the computation
			var newDir = safeVelocity.Normalized();
			// Get the current velocity direction
			var currentDir = _VelocityComponent.Velocity.Normalized();

			// Update velocity component to match
			var halfway = newDir.Lerp(currentDir, 1f - Mathf.Exp(_VelocityComponent.AccelerationCoefficient));
			_VelocityComponent.Velocity = halfway * _VelocityComponent.Velocity.Length();
		}
	}
}

