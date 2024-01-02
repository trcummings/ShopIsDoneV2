using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
	public interface IDeathHandler
	{
		public Command Die();
	}

	public partial class DeathHandler : Node, IDeathHandler
    {
		public virtual Command Die()
		{
			return new Command();
		}
	}
}

