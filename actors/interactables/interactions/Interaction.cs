﻿using System;
using Godot;

namespace ShopIsDone.Interactables.Interactions
{
	[Tool]
	[GlobalClass]
    public partial class Interaction : Node
	{
		[Signal]
		public delegate void FinishedEventHandler();

		public virtual void Execute()
		{
            // Finish MUST be called at the end of every single execution, async code
            // within or no. Here we call it deferred just so we don't get stuck
			// in any weird await situations
            CallDeferred(nameof(Finish));
        }

		protected virtual void Finish()
		{
            EmitSignal(nameof(Finished));
        }
	}
}

