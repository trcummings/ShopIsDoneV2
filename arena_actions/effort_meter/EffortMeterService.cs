using Godot;
using ShopIsDone.ActionPoints;
using ShopIsDone.Cameras;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Actions.Effort
{
    public partial class EffortMeterService : Node, IService
    {
        [Signal]
        public delegate void UpdatedTotalCostEventHandler(int newTotal, int dir);

        [Export]
        private EffortMeter _EffortMeter;

        [Inject]
        private ScreenshakeService _Screenshake;

        private ArenaAction _Action;
        private ActionHandler _ActionHandler;
        private ActionPointHandler _ActionPointHandler;

        public int EffortAmount
        {
            get { return _EffortMeter.CurrentIndex; }
        }

        public bool IsActive
        {
            get { return _IsActive; }
        }
        private bool _IsActive = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            SetProcess(false);
            // Connect
            _EffortMeter.Activated += () => _IsActive = true;
            _EffortMeter.Deactivated += () => _IsActive = false;
            _EffortMeter.Incremented += () => UpdateEffort(1);
            _EffortMeter.Decremented += () => UpdateEffort(-1);
            _EffortMeter.InvalidSelection += () =>
            {
                // A little bit of screenshake just on the x-axis on invalid
                _Screenshake.Shake(
                    ScreenshakeHandler.ShakePayload.ShakeSizes.Mild,
                    ScreenshakeHandler.ShakeAxis.XOnly
                );
            };
        }

        public void Init(ArenaAction action, int initialAp)
        {
            InjectionProvider.Inject(this);

            // Set vars
            _Action = action;

            // Set effort
            // Get the action cost and the prior effort cost (maxed to the pawn's current AP)
            var actionCost = _Action.ActionCost;
            var effortCost = _Action.GetEffortSpent();

            // Init the effort meter with the effort and the pawn's available AP
            _EffortMeter.Init(effortCost, initialAp - actionCost);

            // Emit initial total cost update
            EmitSignal(nameof(UpdatedTotalCost), actionCost + effortCost);

            // TODO: Make it come in real nice
            _EffortMeter.Show();
            // Enable processing
            SetProcess(true);
        }

        public void CleanUp()
        {
            // TODO: Make it exit real nice
            _EffortMeter.Hide();
            // Disable processing
            SetProcess(false);
        }

        public override void _Process(double delta)
        {
            // If we're using the meter, do not process other input
            if (Input.IsActionPressed("effort_meter"))
            {
                if (Input.IsActionJustPressed("effort_meter"))
                {
                    // Activate the meter
                    _EffortMeter.Activate();
                }
                else
                {
                    // Test increment / decrement input
                    if (Input.IsActionJustPressed("move_up") || Input.IsActionJustPressed("move_right"))
                    {
                        _EffortMeter.Increment();
                    }
                    else if (Input.IsActionJustPressed("move_down") || Input.IsActionJustPressed("move_left"))
                    {
                        _EffortMeter.Decrement();
                    }
                }

                return;
            }
            // If we just released though, we can process on the same frame
            else if (Input.IsActionJustReleased("effort_meter"))
            {
                // Deactivate the meter
                _EffortMeter.Deactivate();
            }
        }

        private void UpdateEffort(int dir)
        {
            // Set the new effort in the action
            _Action.SetEffort(_EffortMeter.CurrentIndex);

            // Update the cost diff
            var actionCost = _Action.ActionCost;
            var effortCost = _Action.GetEffortSpent();

            EmitSignal(nameof(UpdatedTotalCost), actionCost + effortCost, dir);
        }
    }
}
