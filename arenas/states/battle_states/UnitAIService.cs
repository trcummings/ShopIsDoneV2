using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShopIsDone.AI;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas
{
    public partial class UnitAIService : UnitTurnService, IService
    {
        [Signal]
        public delegate void FinishedRunningAIEventHandler();

        // State
        private Queue<ActionPlanner> _Units = new Queue<ActionPlanner>();
        private ActionPlanner _CurrentUnit = null;

        public IEnumerable<ActionPlanner> GetAllUnitAI()
        {
            return GetTurnEntities()
                .Where(u => u.HasComponent<ActionPlanner>())
                .Select(u => u.GetComponent<ActionPlanner>());
        }

        public void RunAI()
        {
            // Get all units
            var allAIUnits = GetAllUnitAI();

            // Reset AI
            foreach (var planner in GetAllUnitAI()) planner.ResetPlanner();

            // Clear state
            _Units.Clear();
            _CurrentUnit = null;

            // Set the units as a queue
            _Units = new Queue<ActionPlanner>(allAIUnits);

            // Run the queue
            RunQueue();
        }

        private bool _CurrentUnitCanStillAct()
        {
            return
                _CurrentUnit != null &&
                _CurrentUnit.Entity.IsActive() &&
                _CurrentUnit.CanStillAct();
        }

        private void RunQueue()
        {
            // As long as there's still customers that can act, run the queue
            if (_Units.Count > 0 || _CurrentUnitCanStillAct())
            {
                // Have each unit Think at the beginning of each action loop
                foreach (var customer in _Units) customer.Think();

                // Set current customer at beginning of while loop
                if (_CurrentUnit == null) _CurrentUnit = _Units.Dequeue();

                // If our current customer can still act, then act
                if (_CurrentUnitCanStillAct())
                {
                    // One shot connection to finish action execution
                    var command = _CurrentUnit.Act();
                    command.Connect(
                        nameof(command.Finished),
                        new Callable(this, nameof(RunQueue)),
                        (uint)ConnectFlags.OneShot
                    );
                    command.Execute();

                    return;
                }
                // Otherwise, null out the current customer and loop
                else
                {
                    _CurrentUnit = null;
                    RunQueue();
                }
            }
            // Otherwise, end the turn
            else EmitSignal(nameof(FinishedRunningAI));
        }
    }
}

