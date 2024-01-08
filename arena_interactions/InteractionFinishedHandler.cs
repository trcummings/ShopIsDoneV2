using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ArenaInteractions
{
    public partial class InteractionFinishedHandler : Node
	{
		public virtual Command FinishInteraction()
		{
			return new Command();
		}
	}
}

