using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Arenas.Battles;

namespace ShopIsDone.Arenas.States
{
	public partial class RunningState : State
	{
        [Export]
        private BattlePhaseManager _PhaseManager;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // TODO: Consume phase commands as they come in

            // Start battle phase helper
            _PhaseManager.Init();
        }
    }
}
