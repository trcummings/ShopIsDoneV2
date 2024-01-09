using System;
using Godot;
using ShopIsDone.Actions;
using Godot.Collections;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Arenas.Meddling
{
	// This class describes a single evaluation of if we should meddle in an
	// action or not
    public partial class ActionMeddle : Node
	{
		// Always check if we should meddle no matter what the flag says
        [Export]
        public bool IgnoreFlag = false;

        [Export]
		public string StateFlagName;

		[Export]
		public bool WhenFlagIs = false;

		public virtual bool ShouldMeddle(ArenaAction action, Dictionary<string, Variant> message)
		{
			return false;
		}

		public virtual Command Meddle(ArenaAction action, Dictionary<string, Variant> message)
		{
			return new Command();
		}
	}
}

