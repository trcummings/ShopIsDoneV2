using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.AI;
using SystemGenerics = System.Collections.Generic;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class EnemyActivityBattleState : State
    {
        [Export]
        private BattlePhaseManager _PhaseManager;

        //// Nodes
        //[OnReadyGet]
        //private Control _EnemyActivityUI;

        [Export]
        private UnitAIService _EnemyTurnService;

        // State
        private SystemGenerics.Queue<ActionPlanner> _Customers = new SystemGenerics.Queue<ActionPlanner>();
        private ActionPlanner _CurrentCustomer = null;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Refill enemy AP
            _EnemyTurnService.RefillApToMax();

            // Reset AI
            _EnemyTurnService.ResetAI();

            // Resolve status effects
            //_EnemyTurnSystem.ResolveStatusEffects();

            // Clear state
            _Customers.Clear();
            _CurrentCustomer = null;

            // Set the units as a queue
            _Customers = new SystemGenerics.Queue<ActionPlanner>(
                _EnemyTurnService.GetAllUnitAI()
            );

            // Run the queue
            RunQueue();
        }

        private void RunQueue()
        {
            // Only run the queue if this state has been initialized
            if (!HasBeenInitialized) return;

            // As long as there's still customers that can act, run the queue
            if (_Customers.Count > 0 || (_CurrentCustomer != null && _CurrentCustomer.CanStillAct()))
            {
                // Have each unit Think at the beginning of each action loop
                foreach (var customer in _Customers) customer.Think();

                // Set current customer at beginning of while loop
                if (_CurrentCustomer == null) _CurrentCustomer = _Customers.Dequeue();

                // If our current customer can still act, then act
                if (_CurrentCustomer.CanStillAct() && _CurrentCustomer.Entity.IsActive())
                {
                    // One shot connection to finish action execution
                    var command = _CurrentCustomer.Act();
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
                    _CurrentCustomer = null;
                    RunQueue();
                }
            }
            // Otherwise, end the turn
            else _PhaseManager.AdvanceToNextPhase();
        }
    }
}