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
        public string CurrentState { get { return _CurrentState?.Name ?? ""; } }
        protected State _CurrentState = null;
        private Dictionary<string, Variant> _CurrentStateMessage = new Dictionary<string, Variant>();
        private State _LastState = null;
        private Dictionary<string, Variant> _LastStateMessage = new Dictionary<string, Variant>();

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
                // Give state reference to state machine
                _state.Init(this);
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

        public (string, Dictionary<string, Variant>) GetLastStateProps()
        {
            return (_LastState?.Name ?? "", _LastStateMessage);
        }

        // Input hooks
        public override void _Input(InputEvent @event)
        {
            if (_CurrentState == null || !_CurrentState.CanRunHooks) return;

            _CurrentState.OnInput(@event);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_CurrentState == null || !_CurrentState.CanRunHooks) return;

            _CurrentState.OnUnhandledInput(@event);
        }


        // Process hooks
        public override void _Process(double delta)
        {
            if (_CurrentState == null || !_CurrentState.CanRunHooks) return;

            _CurrentState.UpdateState(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_CurrentState == null || !_CurrentState.CanRunHooks) return;

            _CurrentState.PhysicsUpdateState(delta);
        }

        public void ChangeState(string stateName, Dictionary<string, Variant> message = null)
        {
            if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name}.ChangeState called with {stateName}");
            // If we have a previous state, test state change restrictions
            if (_CurrentState != null)
            {
                // Ignore if it's the same state we were in before
                if (!AllowTransitionsIntoSameState && _CurrentState.Name == stateName) return;

                // Check transitions
                var invalidTransition = !_CurrentState.Transitions.Any((State candidate) => stateName == candidate.Name);

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
            // If we get to this point, there's a problem
            if (GameManager.IsDebugMode() && Debug)
            {
                GD.PrintErr($"No state with {stateName} found in {Name}.ChangeState!");
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
            if (_CurrentState != null)
            {
                // Connect to state's StateExited signal
                // NB: Because we can enter and exit the same states, check for
                // an existing connection
                if (!_CurrentState.IsConnected(nameof(_CurrentState.StateExited), _AfterExitCallable))
                {
                    // Set bound callable to disconnect from afterwards
                    _AfterExitCallable = Callable.From(() => OnAfterExit(nextState, message));
                    // Connect to bound callable
                    _CurrentState.Connect(nameof(_CurrentState.StateExited), _AfterExitCallable);
                }

                // Run state's OnExit hook with the name of the next state
                if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name} exiting {_CurrentState.Name}");
                _CurrentState.OnExit(nextState.Name);
            }
            // Otherwise, skip directly to the after exit function
            else OnAfterExit(nextState, message);
        }

        private void OnAfterExit(State nextState, Dictionary<string, Variant> message = null)
        {
            // Disconnect if connected
            if (_CurrentState?.IsConnected(nameof(_CurrentState.StateExited), _AfterExitCallable) ?? false)
            {
                _CurrentState.Disconnect(nameof(_CurrentState.StateExited), _AfterExitCallable);
            }

            // Update the state tracking vars
            _LastState = _CurrentState;
            _LastStateMessage = _CurrentStateMessage;
            _CurrentStateMessage = message;
            _CurrentState = nextState;

            // Connect to state's StateEntered signal
            // NB: Because we can enter and exit the same states, check for
            // an existing connection
            if (!_CurrentState.IsConnected(nameof(_CurrentState.StateStarted), _AfterStartCallable))
            {
                _CurrentState.Connect(nameof(_CurrentState.StateStarted), _AfterStartCallable);
            }

            // Start the next state
            if (GameManager.IsDebugMode() && Debug) GD.Print($"{Name} entering {_CurrentState.Name}");
            _CurrentState.OnStart(message);
        }

        private void OnAfterStart()
        {
            // Disconnect from started event
            if (_CurrentState.IsConnected(nameof(_CurrentState.StateStarted), _AfterStartCallable))
            {
                _CurrentState.Disconnect(nameof(_CurrentState.StateStarted), _AfterStartCallable);
            }

            // Run state update function
            _CurrentState.OnUpdate();
        }
    }
}