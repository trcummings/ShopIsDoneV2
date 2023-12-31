using System;
using Godot.Collections;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using System.Linq;
using GenericCollections = System.Collections.Generic;

namespace ShopIsDone.AI
{
    // A Turn Plan makes decisions around what actions an AI should plan and execute,
    // among a collection of action plans
    [GlobalClass]
    public partial class TurnPlan : Resource
    {
        [Signal]
        public delegate void TurnFinishedEventHandler();

        [Export]
        private Array<ActionPlan> _ActionPlans;
        public Array<ActionPlan> ActionPlans { get { return _ActionPlans; } }

        protected Dictionary<string, Variant> _Blackboard;
        protected GenericCollections.HashSet<Variant> _FinishedPlans = new GenericCollections.HashSet<Variant>();

        public virtual void Init(Dictionary<string, Variant> blackboard, Array<ArenaAction> actions)
        {
            // Initialize each action plan with the accompanying action
            foreach (var plan in _ActionPlans)
            {
                // Find the action with the matching ID for the plan
                var action = actions
                    .ToList()
                    .Find(a => a.Id == plan.ActionId);
                if (action != null) plan.Init(action, _Blackboard);
            }
        }

        public virtual bool IsValid()
        {
            // If turn plan is finished, it's no longer valid
            if (IsPlanFinished()) return false;

            // A turn plan is valid if any of the remaining actions in it are
            // valid
            return GetRemainingPlans().Any(ap => ap.IsValid());
        }

        public virtual int GetPriority()
        {
            return 1;
        }

        public bool IsPlanFinished()
        {
            return _FinishedPlans.Count == _ActionPlans.Count;
        }

        public void ResetTurnPlan()
        {
            _FinishedPlans.Clear();
        }

        public virtual Command ExecuteNextAction()
        {
            return new ConditionalCommand(
                // Safety check
                () => !IsPlanFinished(),
                // Run next priority action
                GetNextPriorityPlan().ExecuteAction()
            );
        }

        private ActionPlan GetNextPriorityPlan()
        {
            // Sort remaining actions by priority
            return GetRemainingPlans()
                .Where(a => a.IsValid())
                .OrderByDescending(a => a.GetPriority())
                .First();
        }

        private GenericCollections.IEnumerable<ActionPlan> GetRemainingPlans()
        {
            return _ActionPlans
                .Where(plan => !_FinishedPlans.Contains(plan));
        }
    }
}

