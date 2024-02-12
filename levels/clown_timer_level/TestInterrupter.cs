using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Arenas.ClownTimer;

namespace ShopIsDone.Levels.ClownTimerLevel
{
    public partial class TestInterrupter : Node, IClownTimerTick
    {
        [Export]
        private TurnInterrupterService _TurnInterrupterService;

        [Export]
        private LevelEntity _TurnInterrupter;

        [Export]
        private double _TimerMax = 5;

        private double _CurrentTimer = 0;
        private bool _IsRunning = false;
        private ActionHandler _ActionHandler;

        public void StartClownTimer()
        {
            // Do nothing
        }

        public void StopClownTimer()
        {
            // Do nothing
        }


        public void ClownTimerTick(double delta)
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

                // Make sure we have our component
                _ActionHandler = _TurnInterrupter.GetComponent<ActionHandler>();

                // Get Interrupt action
                var action = _ActionHandler.GetAction("interrupt");

                // Oneshot connect to finished interrupting
                _TurnInterrupterService.Connect(
                    nameof(_TurnInterrupterService.FinishedInterrupting),
                    Callable.From(() => _IsRunning = false),
                    (uint)ConnectFlags.OneShot
                );

                // Run action through interrupter
                _ = _TurnInterrupterService.RunInterrupter(action);
            }
        }
    }
}

