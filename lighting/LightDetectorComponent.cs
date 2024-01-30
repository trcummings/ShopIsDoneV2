using System;
using Godot;
using ShopIsDone.Core;

namespace ShopIsDone.Lighting
{
	public partial class LightDetectorComponent : NodeComponent
    {
		[Export]
		public LightDetector Detector;
	}
}

