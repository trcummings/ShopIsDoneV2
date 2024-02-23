using System;
using Godot;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Actions;
using ShopIsDone.ClownRules;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class JudgePhaseBattleState : State
	{
        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private ActionService _ActionService;

        [Export]
        private ClownRulesService _ClownRulesService;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            new SeriesCommand(
                // Run judge turn
                _ClownRulesService.ProcessTurnRules(),
                // Update arena
                _ActionService.PostActionUpdate(),
                // Kick off the next turn
                new ActionCommand(_PhaseManager.AdvanceToNextPhase)
            ).Execute();
        }
    }
}
