using System;
using Godot;
using ShopIsDone.Actions;
using Godot.Collections;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Levels;

namespace ShopIsDone.Arenas.Meddling
{
	// This class describes a single evaluation of if we should meddle in an
	// action or not
    public partial class ActionMeddle : Node
	{
		[Inject]
		private LevelData _LevelData;

		// Always check if we should meddle no matter what the flag says
        [Export]
        private bool _IgnoreFlag = false;

        [Export]
		private string _FlagName;

		[Export]
		private bool _WhenFlagIs = false;

		public bool CanMeddle()
		{
            // IF we ignore the flag, then just return true
            if (_IgnoreFlag) return true;

			// Otherwise, does the flag match our desired state? (if the flag
			// does not exist, default to the opposite of what we want to
			// guarantee a failure)
            return _LevelData.GetFlag(_FlagName, !_WhenFlagIs) == _WhenFlagIs;
        }

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

