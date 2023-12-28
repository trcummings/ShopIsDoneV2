using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;

namespace ShopIsDone.Arenas.Battles.States
{
    public partial class EnemyActivityBattleState : State
    {
        [Export]
        private BattlePhaseManager _PhaseManager;

        //[Export]
        //public Team CustomerTeam;

        //// Nodes
        //[OnReadyGet]
        //private Control _EnemyActivityUI;

        //[OnReadyGet]
        //private EntityTurnSystem _EnemyTurnSystem;

        //// State
        //private Queue<ActionPlannerComponent> _Customers = new Queue<ActionPlannerComponent>();
        //private ActionPlannerComponent _CurrentCustomer = null;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            //// Clear state
            //_Customers.Clear();
            //_CurrentCustomer = null;

            //// Progress NPC spawning (in parallel)
            //await AddToCommandManager(new ParallelCommand(
            //    Arena.GetAllSpawnPoints()
            //        .Select(spawnPoint => spawnPoint.GetComponent<EntitySpawnerComponent>().ProgressSpawner(Arena))
            //        .ToArray()
            //)).ExecuteAsTask();

            //// Refill enemy AP
            //await AddToCommandManager(_EnemyTurnSystem.RefillApToMax()).ExecuteAsTask();

            //// Resolve status effects
            //await AddToCommandManager(_EnemyTurnSystem.ResolveStatusEffects(Arena)).ExecuteAsTask();

            //// Set the units as a queue
            //_Customers = new Queue<ActionPlannerComponent>(
            //    _EnemyTurnSystem
            //        .GetActiveTurnEntities()
            //        .Select(e => e.GetComponent<ActionPlannerComponent>()
            //));

            //// While queue exists
            //while (_Customers.Count > 0 || (_CurrentCustomer != null && _CurrentCustomer.CanStillAct()))
            //{
            //    // Have each unit Think at the beginning of each action loop
            //    foreach (var customer in _Customers) await AddToCommandManager(customer.Think(Arena)).ExecuteAsTask();

            //    // Set current customer at beginning of while loop
            //    if (_CurrentCustomer == null) _CurrentCustomer = _Customers.Dequeue();

            //    // If our current customer can still act, then act
            //    if (_CurrentCustomer.CanStillAct()) await AddToCommandManager(_CurrentCustomer.Act(Arena)).ExecuteAsTask();
            //    // Otherwise, null out the current customer and loop
            //    else _CurrentCustomer = null;
            //}

            //// End the turn
            //await AddToCommandManager(Arena.AdvanceToNextPhase()).ExecuteAsTask();
            _PhaseManager.AdvanceToNextPhase();
        }
    }
}