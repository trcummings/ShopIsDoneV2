using System;
using Godot;

namespace ShopIsDone.Interactables.Interactions
{
	[Tool]
	// Simple timeout during interactions
    public partial class WaitForInteraction : Interaction
	{
		[Export]
		private float _WaitTime = 1f;

        public override void Execute()
        {
            GetTree()
                .CreateTimer(_WaitTime)
                .Connect("timeout", Callable.From(Finish), (uint)ConnectFlags.OneShot);
        }
    }
}

