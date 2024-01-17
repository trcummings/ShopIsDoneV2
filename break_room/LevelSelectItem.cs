using System;
using Godot;

namespace ShopIsDone.BreakRoom
{
	[GlobalClass]
	public partial class LevelSelectItem : Resource
    {
		[Export]
		public string Label;

		[Export]
		public PackedScene LevelScene;
	}
}

