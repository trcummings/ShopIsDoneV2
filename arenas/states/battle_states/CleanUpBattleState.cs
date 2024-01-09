using System;
using Godot;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.Commands;
using Godot.Collections;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class CleanUpBattleState : State
    {
        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private UnitTurnService _PlayerUnitTurnService;

        [Export]
        private UnitTurnService _EnemyUnitTurnService;

        [Export]
        private ArenaOutcomeService _OutcomeService;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            new SeriesCommand(
                new ActionCommand(() =>
                {
                    // Reset all units for the next turn
                    _PlayerUnitTurnService.ResetActions();
                    _EnemyUnitTurnService.ResetActions();
                    // Reset all interactions for the next turn
                    _PlayerUnitTurnService.ResetInteractions();
                    _EnemyUnitTurnService.ResetInteractions();
                }),
                // Check if player was victorious!
                new IfElseCommand(
                    // Exit check
                    _OutcomeService.IsPlayerVictorious,
                    //If so, end the arena!
                    new ActionCommand(_OutcomeService.AdvanceToVictoryPhase),
                    // Otherwise, kick off the next turn
                    new ActionCommand(_PhaseManager.AdvanceToNextPhase)
                )
            ).Execute();
        }
    }
}

