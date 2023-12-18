using Godot;
using System;
using Godot.Collections;
using System.Linq;

namespace Utils.StateMachine
{
    public partial class State : Node
    {
        // State Events
        [Signal]
        public delegate void StateStartedEventHandler();

        [Signal]
        public delegate void StateUpdatedEventHandler();

        [Signal]
        public delegate void StateExitedEventHandler();

        [Signal]
        public delegate void StateChangeRequestedEventHandler(string stateName, Dictionary<string, Variant> message);

        [Export]
        public Array<State> Transitions = new Array<State>();

        // Internal vars
        private bool _HasBeenInitialized = false;
        private bool _OnUpdateHasFired = false;

        // Used by state machine
        public bool CanRunHooks
        {
            get { return _HasBeenInitialized && _OnUpdateHasFired; }
        }

        // Enter hook. Only called once
        public virtual void OnStart(Dictionary<string, Variant> message)
        {
            _HasBeenInitialized = true;
            EmitSignal(nameof(StateStarted));
        }

        // Only called once. Signals that the state has been updated
        public virtual void OnUpdate()
        {
            if (!_HasBeenInitialized) return;

            _OnUpdateHasFired = true;
            EmitSignal(nameof(StateUpdated));
        }

        public virtual void UpdateState(double delta)
        {
            if (!_OnUpdateHasFired) return;
        }

        public virtual void PhysicsUpdateState(double delta)
        {
            if (!_OnUpdateHasFired) return;
        }

        // Input hooks
        public virtual void OnInput(InputEvent @event)
        {
            if (!_HasBeenInitialized) return;
        }

        public virtual void OnUnhandledInput(InputEvent @event)
        {
            if (!_HasBeenInitialized) return;
        }

        // Exit hook
        public virtual void OnExit(string nextState)
        {
            if (!_HasBeenInitialized) return;

            _HasBeenInitialized = false;
            _OnUpdateHasFired = false;
            EmitSignal(nameof(StateExited));
        }

        public void ChangeState(string stateName, Dictionary<string, Variant> message = null)
        {
            EmitSignal(SignalName.StateChangeRequested, stateName, message);
        }
    }
}