using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
	public interface IDeathHandler
	{
		Command Die(ApDamagePayload damagePayload);
	}

	// Null implementation of interface
	public partial class DeathHandler : NodeComponent, IDeathHandler
    {
		public virtual Command Die(ApDamagePayload damagePayload)
		{
			return new Command();
		}
	}
}

