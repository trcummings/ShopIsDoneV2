using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.Meddling
{
	// This is a arena utility which observes actions as they come in, and
	// matches them against conditions to see if it should intervene and run some
	// kind of cutscene or alternative action instead.
	public partial class ActionMeddler : Node
	{
		private Array<ActionMeddle> _Meddles;

        public override void _Ready()
        {
            base._Ready();
			_Meddles = GetChildren().OfType<ActionMeddle>().ToGodotArray();
        }

        public Command MeddleWithAction(ArenaAction action, Dictionary<string, Variant> message, Command next)
		{
			return _Meddles
				.ToList()
                // Find the first meddle whose flag we can evaluate, and if it passes, meddle
                .Find(meddle => CanMeddle(meddle) && meddle.ShouldMeddle(action, message))?
				.Meddle(action, message)
				// Otherwise, return the next command
				?? next;
		}

		private bool CanMeddle(ActionMeddle meddle)
		{
			// IF we ignore the flag, then just return true
			if (meddle.IgnoreFlag) return true;
			// Otehrwise, calculate if the flag matches the arena state (or global state)
			var flagMatches = true;
			return flagMatches;
		}
	}
}

