using Godot;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.StateMachine;
using System;
using System.Linq;
using Godot.Collections;
using ArenaStateConsts = ShopIsDone.Arenas.States.Consts;
using FinishedConsts = ShopIsDone.Arenas.States.Finished.Consts;
using ShopIsDone.Conditions;
using ShopIsDone.Core;

namespace ShopIsDone.Arenas
{
    public partial class ArenaOutcomeService : Node, IService
    {
        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private StateMachine _ArenaStateMachine;

        [Export]
        private ConditionsService _ConditionsService;

        private Array<LevelEntity> _ExitedUnits = new Array<LevelEntity>();

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

        public void ExitUnit(LevelEntity entity)
        {
            _ExitedUnits.Add(entity);
        }

        public bool WasOutcomeReached()
        {
            return IsPlayerVictorious() || WasPlayerDefeated();
        }

        public bool IsPlayerVictorious()
        {
            return _ConditionsService.AllConditionsComplete() && _ExitedUnits.Count > 0;
        }

        public bool WasPlayerDefeated()
        {
            // TODO: Add other conditions here

            // The player must not be victorious in order to be defeated. obviously.
            return !IsPlayerVictorious() && NoMoreActivePlayerUnits();
        }

        private bool NoMoreActivePlayerUnits()
        {
            return _PlayerUnitService
                .GetUnits()
                // All units are either not active or not in the arena
                .All(unit => !unit.IsActive() || !unit.IsInArena());
        }
    }
}

