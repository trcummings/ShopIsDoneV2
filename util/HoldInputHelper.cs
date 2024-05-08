using Godot;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Utils
{
    // This helper takes directional input and repeats it after being held for
    // a short period of time
    public partial class HoldInputHelper : Node
    {
        [Signal]
        public delegate void ActionFiredEventHandler(Vector3 dir);

        // Delay before first repeat
        [Export]
        public float InitialDelay = 0.5f;

        // Delay between subsequent repeats
        [Export]
        public float RepeatRate = 0.15f;

        // Timer to handle the initial delay and repeat rate
        private Timer _Timer;
        private Vector3 _PrevValue = Vector3.Zero;
        private Callable _OnDelayTimeout;
        private Callable _OnRepeatTimeout;

        public override void _Ready()
        {
            _Timer = new Timer();
            AddChild(_Timer);
            _OnDelayTimeout = new Callable(this, nameof(OnDelayTimerTimeout));
            _OnRepeatTimeout = new Callable(this, nameof(OnRepeatTimeout));
        }

        private void OnDelayTimerTimeout()
        {
            // Reset timer
            Reset();

            // Emit
            EmitSignal(nameof(ActionFired), _PrevValue);

            // Connect to the repeat timeout
            _Timer.Connect("timeout", _OnRepeatTimeout);
            _Timer.WaitTime = RepeatRate;
            _Timer.OneShot = false;
            _Timer.Start();
        }

        private void OnRepeatTimeout()
        {
            // Emit
            EmitSignal(nameof(ActionFired), _PrevValue);
        }

        public void Update(Vector3 dir)
        {
            // If the value is 0, cancel any existing timers and disconnect
            if (dir == Vector3.Zero) Reset();

            // Otherwise, we've pressed a new value, emit immediately, and start
            // the initial delay timer
            else if (dir != _PrevValue && IsCardinalDirection(dir))
            {
                // Reset any timers
                Reset();

                // Emit
                EmitSignal(nameof(ActionFired), dir);

                // Start the initial delay timer
                _Timer.WaitTime = InitialDelay;
                _Timer.OneShot = true;
                _Timer.Connect("timeout", _OnDelayTimeout);
                _Timer.Start();
            }

            // Update the value
            _PrevValue = dir;
        }

        private static bool IsCardinalDirection(Vector3 dir)
        {
            return
                dir == Vector3.Back ||
                dir == Vector3.Forward ||
                dir == Vector3.Left ||
                dir == Vector3.Right;
        }

        private void Reset()
        {
            _Timer.Stop();
            _Timer.SafeDisconnect("timeout", _OnDelayTimeout);
            _Timer.SafeDisconnect("timeout", _OnRepeatTimeout);
        }
    }
}