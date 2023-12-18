using Godot;
using System;
using System.Linq;
using Godot.Collections;

namespace Utils.StateMachine
{
    public partial class StateMachine : Node
    {
        // You can enter the same state you just exited
        [Export]
        public bool AllowTransitionsIntoSameState = true;

        // You don't need to define transitions for each state
        [Export]
        public bool IgnoreTransitionRestrictions = true;

        // State vars
        public string CurrentState;
        public string LastState;

        // Reference to state object
        protected State _State = null;

        public override void _Ready()
        {
            foreach (State _state in GetChildren().OfType<State>().ToList())
            {
                // Connect to state change requests
                _state.Connect(nameof(State.StateChangeRequested), new Callable(this, nameof(ChangeState)));
            }
        }

        // Input hooks
        public override void _Input(InputEvent @event)
        {
            if (_State == null || !_State.CanRunHooks) return;

            _State.OnInput(@event);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_State == null || !_State.CanRunHooks) return;

            _State.OnUnhandledInput(@event);
        }


        // Process hooks
        public override void _Process(double delta)
        {
            if (_State == null || !_State.CanRunHooks) return;

            _State.UpdateState(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_State == null || !_State.CanRunHooks) return;

            _State.PhysicsUpdateState(delta);
        }

        public void ChangeState(string stateName, Dictionary<string, Variant> message = null)
        {
            // If we have a previous state, test state change restrictions
            if (_State != null)
            {
                // Ignore if it's the same state we were in before
                if (!AllowTransitionsIntoSameState && _State.Name == stateName) return;

                // Check transitions
                var invalidTransition = !_State.Transitions.Any((State candidate) => stateName == candidate.Name);

                if (!IgnoreTransitionRestrictions && invalidTransition) return;
            }

            // Traverse the states and find the new state, set to that state
            // and emit the state changed signal
            foreach (State _state in GetChildren().OfType<State>().ToList())
            {
                if (stateName == _state.Name)
                {
                    SetState(_state, message);
                    return;
                }
            }
        }

        // Helper methods
        private void SetState(State nextState, Dictionary<string, Variant> message = null)
        {
            // If no next state, return early
            if (nextState == null)
            {
                GD.PrintErr("No next state given for ", Name);
                return;
            }

            // If we have a previous state, run its OnExit callback
            if (_State != null)
            {
                // Connect to state's StateExited signal
                // NB: Because we can enter and exit the same states, check for
                // an existing connection
                if (!_State.IsConnected(nameof(State.StateExited), new Callable(this, nameof(OnAfterExit))))
                {
                    _State.StateExited += () => OnAfterExit(nextState, message);
                }

                // Run state's OnExit hook with the name of the next state
                _State.OnExit(nextState.Name);
            }
            // Otherwise, skip directly to the after exit function
            else OnAfterExit(nextState, message);
        }

        private void OnAfterExit(State nextState, Dictionary<string, Variant> message = null)
        {
            // Disconnect if connected
            if (_State?.IsConnected(nameof(State.StateExited), new Callable(this, nameof(OnAfterExit))) ?? false)
            {
                _State.Disconnect(nameof(State.StateExited), new Callable(this, nameof(OnAfterExit)));
            }

            // Update the state tracking vars
            LastState = CurrentState;
            CurrentState = nextState.Name;

            // Update state reference
            _State = nextState;

            // Connect to state's StateEntered signal
            // NB: Because we can enter and exit the same states, check for
            // an existing connection
            if (!_State.IsConnected(nameof(State.StateStarted), new Callable(this, nameof(OnAfterStart))))
            {
                _State.Connect(nameof(State.StateStarted), new Callable(this, nameof(OnAfterStart)));
            }

            // Start the next state
            _State.OnStart(message);
        }

        private void OnAfterStart()
        {
            // Disconnect from started event
            if (_State.IsConnected(nameof(State.StateStarted), new Callable(this, nameof(OnAfterStart))))
            {
                _State.Disconnect(nameof(State.StateStarted), new Callable(this, nameof(OnAfterStart)));
            }

            // Run state update function
            _State.OnUpdate();
        }
    }
}