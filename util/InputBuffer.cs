using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace ShopIsDone.Utils.Inputs
{
    public partial class InputBuffer : Node
    {
        private class InputActionInfo
        {
            public string ActionName;
            public float TimeStamp; // Timestamp in seconds
            public enum State { JustPressed, StillPressed, JustReleased }
            public State CurrentState;
        }

        private List<InputActionInfo> _InputActions = new List<InputActionInfo>();

        [Export]
        private float _TimeFrame = 1.0f; // Time frame in seconds to keep inputs

        [Export]
        private Array<string> _Actions = new Array<string>();
        public Array<string> Actions { get { return _Actions; } }

        public override void _Process(double delta)
        {
            var currentTime = Time.GetTicksMsec() / 1000.0f;

            // Update existing action states before adding new ones to avoid duplicates
            foreach (var actionInfo in _InputActions)
            {
                if (
                    Input.IsActionPressed(actionInfo.ActionName) &&
                    actionInfo.CurrentState == InputActionInfo.State.JustPressed
                )
                {
                    actionInfo.CurrentState = InputActionInfo.State.StillPressed;
                }
                else if (
                    !Input.IsActionPressed(actionInfo.ActionName) &&
                    actionInfo.CurrentState == InputActionInfo.State.StillPressed
                )
                {
                    actionInfo.CurrentState = InputActionInfo.State.JustReleased;
                    actionInfo.TimeStamp = currentTime; // Update timestamp to the release time
                }
            }

            // Check and add new just pressed actions
            foreach (var action in _Actions)
            {
                var actionName = action.ToString();
                if (
                    Input.IsActionJustPressed(actionName) &&
                    !_InputActions.Any(a =>
                        a.ActionName == actionName &&
                        (
                            a.CurrentState == InputActionInfo.State.JustPressed ||
                            a.CurrentState == InputActionInfo.State.StillPressed
                        )
                    )
                )
                {
                    _InputActions.Add(new InputActionInfo
                    {
                        ActionName = actionName,
                        TimeStamp = currentTime,
                        CurrentState = InputActionInfo.State.JustPressed
                    });
                }
            }

            // Cleanup old input actions outside the time frame
            _InputActions.RemoveAll(action => currentTime - action.TimeStamp > _TimeFrame);

            // Optionally, remove JustReleased actions if you only want them for immediate checks
            _InputActions.RemoveAll(action => action.CurrentState == InputActionInfo.State.JustReleased && currentTime - action.TimeStamp > delta);
        }

        private bool IsInputInState(string actionName, InputActionInfo.State state)
        {
            return _InputActions.Any(action => action.ActionName == actionName && action.CurrentState == state);
        }

        public bool IsActionJustPressed(string actionName)
        {
            return IsInputInState(actionName, InputActionInfo.State.JustPressed);
        }

        public bool IsActionStillPressed(string actionName)
        {
            return IsInputInState(actionName, InputActionInfo.State.StillPressed);
        }

        public bool IsActionJustReleased(string actionName)
        {
            return IsInputInState(actionName, InputActionInfo.State.JustReleased);
        }

        // Method to check if an action was just pressed within X milliseconds
        public bool WasInputJustPressedWithin(string actionName, float withinMsec)
        {
            float currentTime = Time.GetTicksMsec() / 1000.0f;
            return Input.IsActionJustPressed(actionName) || _InputActions.Any(action =>
                action.ActionName == actionName &&
                action.CurrentState == InputActionInfo.State.JustPressed &&
                currentTime - action.TimeStamp <= withinMsec / 1000.0f
            );
        }

        public string GetMostRecentlyJustPressedAction(IEnumerable<string> actionNames)
        {
            var relevantActions = _InputActions
                .Where(action =>
                    Input.IsActionJustPressed(action.ActionName) || (
                    actionNames.Contains(action.ActionName) &&
                    action.CurrentState == InputActionInfo.State.JustPressed
                ))
                .OrderByDescending(action => action.TimeStamp)
                .FirstOrDefault();

            return relevantActions?.ActionName;
        }
    }
}