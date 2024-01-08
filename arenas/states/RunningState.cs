using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Arenas.Battles;
using ShopIsDone.Pausing;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.States
{
	public partial class RunningState : State
	{
        [Export]
        private BattlePhaseManager _PhaseManager;

        [Inject]
        private PauseInputHandler _PauseInputHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Enable Pausing
            _PauseInputHandler.IsActive = true;

            // Start battle phase helper
            _PhaseManager.Init();
        }

        public override void OnExit(string nextState)
        {
            // Stop phase manager
            _PhaseManager.Stop();

            base.OnExit(nextState);
        }
    }
}
