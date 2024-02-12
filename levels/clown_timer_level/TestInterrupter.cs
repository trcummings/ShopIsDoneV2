using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Arenas.ClownTimer;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Levels.ClownTimerLevel
{
    public partial class TestInterrupter : NodeComponent, IClownTimerTick
    {
        [Export]
        private ActionHandler _ActionHandler;

        [Export]
        private double _TimerMax = 5;

        [Inject]
        private TurnInterrupterService _TurnInterrupterService;

        // State
        private double _CurrentTimer = 0;
        private bool _IsRunning = false;

        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

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

