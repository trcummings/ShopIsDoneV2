using Godot;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Lighting
{
	// This service is used for turning off and on lights in the state en masse
	public partial class ArenaLightService : Node, IService
    {
		[Export]
		public Color ClownColor;

		public void SetAllLightsToColor()
		{

		}

		public void RevertAllLights()
		{

		}
	}
}
