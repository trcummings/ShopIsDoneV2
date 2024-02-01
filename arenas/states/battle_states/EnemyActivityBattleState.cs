using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.AI;
using SystemGenerics = System.Collections.Generic;
using ShopIsDone.Entities.BehindSpirit;

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

        [Export]
        private BehindSpiritService _BehindSpiritService;

        // State
        private SystemGenerics.Queue<ActionPlanner> _Customers = new SystemGenerics.Queue<ActionPlanner>();
        private ActionPlanner _CurrentCustomer = null;

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Execute behind spirit turn
            if (_BehindSpiritService.CanExecute())
            {
                var command = _BehindSpiritService.Execute();
                command.CallDeferred(nameof(command.Execute));
                await ToSignal(command, nameof(command.Finished));
            }

            // Refill enemy AP
            _EnemyTurnService.RefillApToMax();

            // Resolve status effects
            //_EnemyTurnSystem.ResolveStatusEffects();

            // Run enemy AI
            _EnemyTurnService.Connect(
                nameof(_EnemyTurnService.FinishedRunningAI),
                new Callable(_PhaseManager, nameof(_PhaseManager.AdvanceToNextPhase)),
                (uint)ConnectFlags.OneShot
            );
            _EnemyTurnService.RunAI();
        }
    }
}