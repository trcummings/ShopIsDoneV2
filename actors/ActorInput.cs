using System;
using Godot;

namespace ShopIsDone.Actors
{
	public interface IActorInput
	{
        Vector3 MoveDir { get; set; }

		void UpdateInput();
    }

	public partial class ActorInput : Resource, IActorInput
	{
		public Vector3 MoveDir
		{
			get { return Vector3.Zero; }
			// No set implementation
			set { }
		}

		public void UpdateInput()
		{
			// Do nothing
		}
	}
}

