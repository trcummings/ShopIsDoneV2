using System;
using Godot;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Actions;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class CleanUpBattleState : State
    {
        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private Arena _Arena;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            new SeriesCommand(
                // Reset all units and tiles for the next turn
                new SeriesCommand(
                    GetTree()
                        .GetNodesInGroup("entities")
                        .OfType<LevelEntity>()
                        .Where(e => e.HasComponent<ActionHandler>())
                        .Select(e => e.GetComponent<ActionHandler>())
                        .Select(ah => new ActionCommand(ah.ResetActions))
                        .ToArray()
                ),

                // Check if all player units have exited
                new IfElseCommand(
                    // Exit check
                    _Arena.IsPlayerVictorious,
                    //If so, end the arena!
                    _Arena.AdvanceToVictoryPhase(),
                    // Otherwise, kick off the next turn
                    new ActionCommand(_PhaseManager.AdvanceToNextPhase)
                )
            ).Execute();
        }
    }
}

