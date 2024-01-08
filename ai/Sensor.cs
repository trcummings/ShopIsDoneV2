using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.AI
{
    public partial class Sensor : Node3D
	{
		public void Init()
		{
			InjectionProvider.Inject(this);
		}

		public virtual void Sense(LevelEntity entity, Dictionary<string, Variant> blackboard)
		{
			// Do nothing
		}
	}
}

