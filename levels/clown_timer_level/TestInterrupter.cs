using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Arenas.Battles.States;
using ShopIsDone.Core;
using System;
using System.Threading.Tasks;
using Godot.Collections;

namespace ShopIsDone.Levels.ClownTimerLevel
{
    public partial class TestInterrupter : Node
    {
        [Signal]
        public delegate void FinishedInterruptingEventHandler();

        [Export]
        private PlayerTurnBattleState _PlayerTurnBattleState;

        [Export]
        private ActionService _ActionService;

        [Export]
        private LevelEntity _TurnInterrupter;

        [Export]
        private double _TimerMax = 5;

        private double _CurrentTimer = 0;
        private bool _IsRunning = false;
        private ActionHandler _ActionHandler;

        public void OnTimerTick(double delta)
        {
            // Ignore if running
            if (_IsRunning) return;

            // Update timer
            _CurrentTimer += delta;

            // If we've maxed out, start running
            if (_CurrentTimer >= _TimerMax)
            {
                // Set running and clear timer
                _IsRunning = true;
                _CurrentTimer = 0;

                // Oneshot connect to finished interrupting
                Connect(
                    nameof(FinishedInterrupting),
                    Callable.From(() => _IsRunning = false),
                    (uint)ConnectFlags.OneShot
                );
                _ = RunInterrupter();
            }
        }

        private async Task RunInterrupter()
        {
            // Make sure we have our component
            _ActionHandler = _TurnInterrupter.GetComponent<ActionHandler>();

            // Get action
            var action = _ActionHandler.GetAction("interrupt");

            // Interrupt the player's turn
            _PlayerTurnBattleState.Interrupt();

            // Run the action
            var command = _ActionService.ExecuteAction(action, new Dictionary<string, Variant>());
            command.CallDeferred(nameof(command.Execute));
            await ToSignal(command, nameof(command.Finished));

            // Resume the player's turn
            _PlayerTurnBattleState.Resume();

            // Finish
            EmitSignal(nameof(FinishedInterrupting));
        }
    }
}

