using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils;

namespace ShopIsDone.Arenas.Meddling
{
	// This is a arena utility which observes actions as they come in, and
	// matches them against conditions to see if it should intervene and run some
	// kind of cutscene or alternative action instead.
	public partial class ActionMeddler : Node, IService, IInitializable
    {
		[Export]
		private bool _IsActive = true;

		private Array<ActionMeddle> _Meddles;

        public override void _Ready()
        {
            base._Ready();
			_Meddles = GetChildren().OfType<ActionMeddle>().ToGodotArray();
        }

		public void Init()
		{
			// Inject each meddle with its dependencies
			foreach (var meddle in _Meddles) InjectionProvider.Inject(meddle);
		}

        public Command MeddleWithAction(ArenaAction action, Dictionary<string, Variant> message, Command next)
		{
			// Ignore if not active
			if (!_IsActive) return next;

			// Otherwise, find the first meddle whose flag we can evaluate, and
			// if it passes, meddle
            return _Meddles
				.ToList()
                .Find(meddle => meddle.CanMeddle() && meddle.ShouldMeddle(action, message))?
				.Meddle(action, message)
				// If no meddle applies, return the next command
				?? next;
		}
	}
}

