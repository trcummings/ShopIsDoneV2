using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Core;
using System.Linq;
using System.Threading.Tasks;

namespace ShopIsDone.EntityStates
{
    public partial class EntityStateHandler : NodeComponent, IEntityActiveHandler
    {
        [Signal]
        public delegate void ChangedStateEventHandler();

        [Signal]
        public delegate void PushedStateEventHandler();

        [Export]
        public EntityState InitialState;

        private EntityState _CurrentState;

        private Dictionary<string, EntityState> _States;

        public override void _Ready()
        {
            base._Ready();
            // Get all states
            _States = GetChildren()
                .OfType<EntityState>()
                .Aggregate(new Dictionary<string, EntityState>(), (seed, state) =>
                {
                    seed.Add(state.Id, state);
                    return seed;
                });
            _CurrentState = InitialState;
            _CurrentState.Enter();
        }

        // Public API
        public EntityState CurrentState
        {
            get { return _CurrentState; }
            set { _CurrentState = value; }
        }

        public bool IsInState(string state)
        {
            return _CurrentState.Id == state;
        }

        public bool IsInArena()
        {
            return _CurrentState.IsInArena();
        }

        public bool IsActive()
        {
            return _CurrentState.CanAct();
        }

        //public override void Init()
        //{
        //    _CurrentState = InitialState;
        //    _CurrentState.Enter();
        //}

        public void ChangeState(string state)
        {
            Task _ = ChangeStateAsync(state);
        }

        public void PushState(string state)
        {
            Task _ = PushStateAsync(state);
        }

        public async Task ChangeStateAsync(string state)
        {
            if (!_States.ContainsKey(state))
            {
                GD.PrintErr($"No state named {state} in EntityStateHandler!");
                return;
            }

            // Grab new state
            var newState = _States[state];

            // Exit current state
            _CurrentState.Exit();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateExited));


            // Enter next state
            _CurrentState = newState;
            _CurrentState.Enter();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateEntered));

            EmitSignal(nameof(ChangedState));
        }

        // Pushes a state on, enters, immediately exits
        private async Task PushStateAsync(string state)
        {
            if (!_States.ContainsKey(state))
            {
                GD.PrintErr($"No state named {state} in EntityStateHandler!");
                return;
            }

            // Grab new state
            var pushState = _States[state];

            // Exit current state
            _CurrentState.Exit();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateExited));

            // Push state on, enter then exit
            pushState.Enter();
            await ToSignal(pushState, nameof(_CurrentState.StateEntered));
            pushState.Exit();
            await ToSignal(pushState, nameof(_CurrentState.StateExited));

            // Re-enter current state
            _CurrentState.Enter();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateEntered));

            EmitSignal(nameof(PushedState));
        }
    }
}
