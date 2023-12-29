using Godot;
using System;
using System.Linq;
using Godot.Collections;
using ShopIsDone.Game;

namespace ShopIsDone.Utils.StateMachine
{
    public partial class StateMachine : Node
    {
        // You can enter the same state you just exited
        [Export]
        public bool AllowTransitionsIntoSameState = true;

        // You don't need to define transitions for each state
        [Export]
        public bool IgnoreTransitionRestrictions = true;

        [Export]
        public bool Debug = false;

        // State vars
        public string CurrentState;
        public string LastState;

        // Reference to state object
        protected State _State = null;

        public Array<State> States { get { return _States; } }
        private Array<State> _States = new Array<State>();

        private Callable _AfterExitCallable;
        private Callable _AfterStartCallable;

        public override void _Ready()
        {
            _AfterExitCallable = new Callable(this, nameof(OnAfterExit));
            _AfterStartCallable = new Callable(this, nameof(OnAfterStart));
            foreach (State _state in GetChildren().OfType<State>().ToList())
            {
                _States.Add(_state);
                // Connect to state change requests
                _state.StateChangeRequested += ChangeState;
            }
        }

        public State GetState(string stateName)
        {
            var state = _States.ToList().Find(state => state.Name == stateName);
            if (state == null) GD.PrintErr($"No state with {stateName} in states!");
            return state;
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
            if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name}.ChangeState called with {stateName}");
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
            foreach (State _state in _States)
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
                if (!_State.IsConnected(nameof(_State.StateExited), _AfterExitCallable))
                {
                    // Set bound callable to disconnect from afterwards
                    _AfterExitCallable = Callable.From(() => OnAfterExit(nextState, message));
                    // Connect to bound callable
                    _State.Connect(nameof(_State.StateExited), _AfterExitCallable);
                }

                // Run state's OnExit hook with the name of the next state
                if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name} exiting {_State.Name}");
                _State.OnExit(nextState.Name);
            }
            // Otherwise, skip directly to the after exit function
            else OnAfterExit(nextState, message);
        }

        private void OnAfterExit(State nextState, Dictionary<string, Variant> message = null)
        {
            // Disconnect if connected
            if (_State?.IsConnected(nameof(_State.StateExited), _AfterExitCallable) ?? false)
            {
                _State.Disconnect(nameof(_State.StateExited), _AfterExitCallable);
            }

            // Update the state tracking vars
            CurrentState = nextState.Name;
            _State = nextState;

            // Connect to state's StateEntered signal
            // NB: Because we can enter and exit the same states, check for
            // an existing connection
            if (!_State.IsConnected(nameof(_State.StateStarted), _AfterStartCallable))
            {
                _State.Connect(nameof(_State.StateStarted), _AfterStartCallable);
            }

            // Start the next state
            if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name} entering {_State.Name}");
            _State.OnStart(message);
        }

        private void OnAfterStart()
        {
            // Disconnect from started event
            if (_State.IsConnected(nameof(_State.StateStarted), _AfterStartCallable))
            {
                _State.Disconnect(nameof(_State.StateStarted), _AfterStartCallable);
            }

            // Run state update function
            _State.OnUpdate();
        }
    }
}