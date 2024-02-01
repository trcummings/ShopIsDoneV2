using System;
using Godot;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Actions;

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

        [Export]
        private Arena _Arena;

        [Export]
        private ActionService _ActionService;

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
                ),
                // Clean up for all entities
                new SeriesCommand(
                    GetTree()
                        .GetNodesInGroup("entities")
                        .OfType<LevelEntity>()
                        .Where(_Arena.IsAncestorOf)
                        .Select(e => e.OnCleanUp())
                        .ToArray()
                ),
                new WaitIdleFrameCommand(this),
                // Update arena
                _ActionService.PostActionUpdate()
            ).Execute();
        }
    }
}

