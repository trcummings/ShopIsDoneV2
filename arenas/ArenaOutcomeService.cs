using Godot;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.StateMachine;
using System;
using System.Linq;
using Godot.Collections;
using ArenaStateConsts = ShopIsDone.Arenas.States.Consts;
using FinishedConsts = ShopIsDone.Arenas.States.Finished.Consts;

namespace ShopIsDone.Arenas
{
    public partial class ArenaOutcomeService : Node, IService
    {
        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private StateMachine _ArenaStateMachine;

        public void AdvanceToVictoryPhase()
        {
            _ArenaStateMachine.ChangeState(ArenaStateConsts.FINISHED, new Dictionary<string, Variant>()
            {
                { FinishedConsts.FINISHED_STATE_KEY, FinishedConsts.States.VICTORY }
            });
        }

        public void AdvanceToDefeatPhase()
        {
            _ArenaStateMachine.ChangeState(ArenaStateConsts.FINISHED, new Dictionary<string, Variant>()
            {
                { FinishedConsts.FINISHED_STATE_KEY, FinishedConsts.States.FAILURE }
            });
        }

        public bool WasOutcomeReached()
        {
            return IsPlayerVictorious() || WasPlayerDefeated();
        }

        public bool IsPlayerVictorious()
        {
            return false;
        }

        public bool WasPlayerDefeated()
        {
            // TODO: Add other conditions here


            // The player must not be victorious in order to be defeated. obviously.
            var playerNotVictorious = !IsPlayerVictorious();

            // No more active player pawns in the arena
            var noMoreActivePlayerPawnsInArena = _PlayerUnitService
                .GetUnits()
                // All units are either not active or not in the arena
                .All(unit => !unit.IsActive() || !unit.IsInArena());

            return playerNotVictorious && noMoreActivePlayerPawnsInArena;
        }
    }
}

