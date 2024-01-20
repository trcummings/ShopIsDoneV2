using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Battles;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using System;
using System.Linq;

namespace ShopIsDone.Arenas.ClownTimer
{
    // This service tracks a timer that ticks up while the player is taking their
    // turn, it can be used by timer events to mess around with the player's turn
    public partial class ClownTimerService : Node, IService
    {
        [Signal]
        public delegate void ClownTimerUpdateEventHandler(double delta);

        [Export]
        private bool _IsActive = false;

        [Export]
        private ActionService _ActionService;

        [Export]
        private BattlePhaseManager _PhaseManager;

        [Export]
        private State _PlayerTurnState;

        [Export]
        private StateMachine _PlayerTurnStateMachine;

        // States in the player turn that the timer can run during. We don't
        // want the timer to run during
        [Export]
        private Array<State> _WhitelistedPlayerTurnStates;

        // State
        private double _Timer = 0;

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Do not process if the clown timer is not active
            if (!_IsActive) return;

            // Do not process if the action service is running an action
            if (_ActionService.HasCurrentAction()) return;

            // Do not process if we are not in the player turn state
            if (_PhaseManager.CurrentPhase != _PlayerTurnState) return;

            // Do not process if we're not in one of the whitelisted player turn
            // states
            var whitelistedNames = _WhitelistedPlayerTurnStates.Select(s => s.Name.ToString());
            if (!whitelistedNames.Contains(_PlayerTurnStateMachine.CurrentState)) return;

            // Update the timer, and then emit
            _Timer += delta;
            EmitSignal(nameof(ClownTimerUpdate), _Timer);
        }
    }
}

