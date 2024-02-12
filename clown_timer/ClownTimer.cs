using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Battles;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using System;
using System.Linq;
using ShopIsDone.Utils;
using Generics = System.Collections.Generic;

namespace ShopIsDone.Arenas.ClownTimer
{
    public interface IClownTimerTick
    {
        void ClownTimerTick(double delta);

        void StartClownTimer();

        void StopClownTimer();
    }

    // This service tracks a timer that ticks up while the player is taking their
    // turn, it can be used by timer events to mess around with the player's turn
    public partial class ClownTimer : Node, IService, IInitializable
    {
        [Signal]
        public delegate void ClownTimerStartedEventHandler();

        [Signal]
        public delegate void ClownTimerUpdateEventHandler(double delta);

        [Signal]
        public delegate void ClownTimerStoppedEventHandler();

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
        private bool _TickedLastTurn = false;
        private double _Timer = 0;

        public void Init()
        {
            // Initialize the timer
            _Timer = 0;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // Determine if we should not tick
            var shouldNotTick =
                // Do not process if the clown timer is not active
                !_IsActive ||
                // Do not process if the action service is running an action
                _ActionService.HasCurrentAction() ||
                // Do not process if we are not in the player turn state
                _PhaseManager.CurrentPhase != _PlayerTurnState ||
                // Do not process if we're not in a whitelisted player turn states
                !IsInWhitelistedState();

            if (shouldNotTick)
            {
                // If we ticked last turn, we're now stopping the timer
                if (_TickedLastTurn) StopClownClock();

                // Set was ticked
                _TickedLastTurn = false;

                // Return early
                return;
            }

            // If we did not tick last turn, we're starting the clock
            if (!_TickedLastTurn) StartClownClock();

            // Update ticked this turn
            _TickedLastTurn = true;

            // Update the timer
            _Timer += delta;

            // Tick the clown clock
            TickClownClock(delta);
        }

        private bool IsInWhitelistedState()
        {
            var whitelistedNames = _WhitelistedPlayerTurnStates.Select(s => s.Name.ToString());
            return whitelistedNames.Contains(_PlayerTurnStateMachine.CurrentState);
        }

        private void TickClownClock(double delta)
        {
            // Tick all tickers
            foreach (var ticker in GetTickers()) ticker.ClownTimerTick(delta);
            // Emit
            EmitSignal(nameof(ClownTimerUpdate), delta);
        }

        private void StartClownClock()
        {
            foreach (var ticker in GetTickers()) ticker.StartClownTimer();
            // Emit
            EmitSignal(nameof(ClownTimerStarted));
        }

        private void StopClownClock()
        {
            foreach (var ticker in GetTickers()) ticker.StopClownTimer();
            // Emit
            EmitSignal(nameof(ClownTimerStopped));
        }

        private Generics.IEnumerable<IClownTimerTick> GetTickers()
        {
            return GetTree().GetNodesInGroup("clown_timer_tick").OfType<IClownTimerTick>();
        }
    }
}

