using Godot;
using System;
using System.Linq;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Actions
{
    public partial class ActionHandler : NodeComponent
    {
        public partial class ActionStateData : GodotObject
        {
            public bool UsedThisTurn = false;

            public int EffortSpent = 0;
        }

        // The actions the pawn has available to use
        [Export]
        public Array<ArenaAction> Actions = new Array<ArenaAction>();

        protected Dictionary<string, ActionStateData> _ActionStates = new Dictionary<string, ActionStateData>();

        protected bool _IsTurnEnded = false;

        [Export]
        private ActionPointHandler _ActionPointHandler;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        // Public API
        public override void Init()
        {
            // Duplicate actions
            Actions = Actions.Duplicate();
            // Initialize with pawn
            foreach (var action in Actions)
            {
                // Inject into action
                _InjectionProvider.InjectObject(action);
                // Init object
                action.Init(this);
                // Add action state
                _ActionStates.Add(action.Id, new ActionStateData());
            }
        }

        public virtual bool IsActionAvailable(ArenaAction action)
        {
            // Get action state
            var actionState = _ActionStates.GetValueOrDefault(action.Id, new ActionStateData());
            return
                // If it wasn't used or we can use it multiple times
                (!actionState.UsedThisTurn || action.CanBeUsedMultipleTimes)
                // ...But we can also afford it
                && _ActionPointHandler.HasEnoughAPForAction(action.ActionCost);
        }

        public int GetTotalActionCost(string id)
        {
            return
                GetActionState(id).EffortSpent +
                (Actions.ToList().Find(a => a.Id == id)?.ActionCost ?? 0);
        }

        public ActionStateData GetActionState(string id)
        {
            return _ActionStates.GetValueOrDefault(id, new ActionStateData());
        }

        public void EndTurn()
        {
            _IsTurnEnded = true;
        }

        public void SetActionUsed(string id)
        {
            GetActionState(id).UsedThisTurn = true;
        }

        public void SetActionEffort(string id, int effort)
        {
            GetActionState(id).EffortSpent = effort;
        }

        public virtual void ResetActions()
        {
            foreach (var key in _ActionStates.Keys)
            {
                var state = _ActionStates[key];
                _ActionStates[key] = new ActionStateData()
                {
                    EffortSpent = state.EffortSpent,
                    UsedThisTurn = false
                };
            }
            _IsTurnEnded = false;
        }

        public virtual bool HasAvailableActions()
        {
            if (_IsTurnEnded) return false;

            // Otherwise, check if we have any actions that we can still use
            return Actions.Any(IsActionAvailable);
        }

        public System.Collections.Generic.IEnumerable<ArenaAction> GetAvailableActions()
        {
            return Actions.Where(IsActionAvailable);
        }

        public ArenaAction GetAction(string id)
        {
            return Actions.Where(action => action.Id == id).First();
        }
    }
}

