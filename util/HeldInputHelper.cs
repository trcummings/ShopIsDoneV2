using System;
using Godot;

namespace ShopIsDone.Utils
{
    [Tool]
    public partial class HeldInputHelper : Node
    {
        [Signal]
        public delegate void JustPressedEventHandler();

        [Signal]
        public delegate void JustReleasedEventHandler();

        [Signal]
        public delegate void HeldForEventHandler(float heldTime);

        [Export]
        public StringName ActionName;

        // State
        public float HeldTime { get { return _HeldTime; } }
        private float _HeldTime = 0.0f;

        public bool WasHeldFor(float time)
        {
            return _HeldTime >= time;
        }

        public void ResetHeldTime()
        {
            _HeldTime = 0.0f;
        }

        public override void _Process(double delta)
        {
            // Ignore if no action name
            if (string.IsNullOrEmpty(ActionName) || Engine.IsEditorHint()) return;

            // If action is currently pressed, accrue held time and emit
            if (Input.IsActionPressed(ActionName))
            {
                _HeldTime += (float)delta;
                EmitSignal(nameof(HeldFor), _HeldTime);
            }
            // Otherwise, reset
            else ResetHeldTime();

            // Emit pressed and released signals
            if (Input.IsActionJustPressed(ActionName)) EmitSignal(nameof(JustPressed));
            if (Input.IsActionJustReleased(ActionName)) EmitSignal(nameof(JustReleased));
        }


        // Editor warning
        public override string[] _GetConfigurationWarnings()
        {
            if (string.IsNullOrEmpty(ActionName)) return new string[] { "Action name not set!" };

            if (!InputMap.HasAction(ActionName)) return new string[] { $"Action name {ActionName} not in InputMap!" };

            return new string[] {};
        }
    }
}

