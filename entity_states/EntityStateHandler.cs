using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using System.Linq;
using System.Threading.Tasks;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Game;
using ShopIsDone.Models;
using ShopIsDone.Utils.DependencyInjection;

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

        [Export]
        private ModelComponent _ModelComponent;

        [Export]
        public bool Debug = false;

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
        }

        public override void Init()
        {
            foreach (var state in _States.Values)
            {
                // Inject state
                InjectionProvider.Inject(state);
                // Initialize state
                state.Init(Entity, this, _ModelComponent);
            }
            _CurrentState = InitialState;
            // After enter, idle
            _CurrentState.Connect(
                nameof(_CurrentState.StateEntered),
                Callable.From(_CurrentState.Idle),
                (uint)ConnectFlags.OneShot
            );
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
            return _CurrentState.IsInArena;
        }

        public bool IsActive()
        {
            return _CurrentState.IsActive;
        }

        public Command RunIdleCurrentState()
        {
            return new ActionCommand(IdleCurrentState);
        }

        public void IdleCurrentState()
        {
            _CurrentState?.Idle();
        }

        public void ChangeState(string state, Dictionary<string, Variant> message = null)
        {
            Task _ = ChangeStateAsync(state, message);
        }

        public void PushState(string state, Dictionary<string, Variant> message = null)
        {
            Task _ = PushStateAsync(state, message);
        }

        public Command RunChangeState(string state, Dictionary<string, Variant> message = null)
        {
            return new AsyncCommand(() => ChangeStateAsync(state, message));
        }

        public Command RunPushState(string state, Dictionary<string, Variant> message = null)
        {
            return new AsyncCommand(() => PushStateAsync(state, message));
        }

        private async Task ChangeStateAsync(string state, Dictionary<string, Variant> message = null)
        {
            if (!_States.ContainsKey(state))
            {
                GD.PrintErr($"No state named {state} in {Entity.Id}'s EntityStateHandler!");
                CallDeferred(nameof(EmitSignal), nameof(ChangedState));
                return;
            }

            // Grab new state
            var newState = _States[state];

            // Exit current state
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.ChangeStateAsync exiting {_CurrentState.Id} State");
            }
            _CurrentState.Exit();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateExited));


            // Enter next state
            _CurrentState = newState;
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.ChangeStateAsync entering {state} State");
            }
            _CurrentState.Enter(message);
            await ToSignal(_CurrentState, nameof(_CurrentState.StateEntered));

            // Idle the new state
            _CurrentState.Idle();

            EmitSignal(nameof(ChangedState));
        }

        // Pushes a state on, enters, immediately exits
        private async Task PushStateAsync(string state, Dictionary<string, Variant> message = null)
        {
            if (!_States.ContainsKey(state))
            {
                GD.PrintErr($"No state named {state} in {Entity.Id}'s EntityStateHandler!");
                CallDeferred(nameof(EmitSignal), nameof(PushedState));
                return;
            }

            // Exit current state
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.PushStateAsync exiting {_CurrentState.Id} State");
            }
            _CurrentState.Exit();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateExited));

            // Grab push state
            var pushState = _States[state];

            // Enter pushed state
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.PushStateAsync push-entering {state} State");
            }
            pushState.Enter(message);
            await ToSignal(pushState, nameof(_CurrentState.StateEntered));

            // Exit pushed state
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.PushStateAsync push-exiting {state} State");
            }
            pushState.Exit();
            await ToSignal(pushState, nameof(_CurrentState.StateExited));

            // Re-enter current state
            if (Debug && GameManager.IsDebugMode())
            {
                GD.Print($"{Entity.Id}'s {Name}.PushStateAsync re-entering {_CurrentState.Id} State");
            }
            _CurrentState.Enter();
            await ToSignal(_CurrentState, nameof(_CurrentState.StateEntered));

            // Emit
            EmitSignal(nameof(PushedState));
        }
    }
}
