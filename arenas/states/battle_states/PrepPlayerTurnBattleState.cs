using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class PrepPlayerTurnBattleState : State
	{
        [Export]
        private BattlePhaseManager _PhaseManager;
        // Update silhouettes

        // Refill AP

        // Resolve status effects

        // Resolve in progress tasks

        // Advance
        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);
            _PhaseManager.AdvanceToNextPhase();
        }
    }
}
