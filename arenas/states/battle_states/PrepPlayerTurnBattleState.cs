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

        [Export]
        private UnitTurnService _PlayerUnitTurnService;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Update silhouettes

            // Refill AP
            _PlayerUnitTurnService.RefillApToMax();

            // Resolve status effects

            // Resolve in progress tasks

            // Advance
            _PhaseManager.AdvanceToNextPhase();
        }
    }
}
