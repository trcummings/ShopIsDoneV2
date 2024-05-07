using Godot;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Entities.BehindSpirit;
using System.Linq;
using ShopIsDone.Entities.EntitySpawner;

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
        private ArenaEntitiesService _EntitiesService;

        [Export]
        private UnitAIService _EnemyTurnService;

        [Export]
        private BehindSpiritService _BehindSpiritService;

        public async override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Execute behind spirit turn
            if (_BehindSpiritService.CanExecute())
            {
                await _BehindSpiritService.Execute().AwaitCommandFrom(this);
            }

            // Progress spawners
            var spawners = _EntitiesService
                .GetAllArenaEntities()
                .Where(e => e.IsActive() && e.IsInArena())
                .Where(e => e.HasComponent<EntitySpawnerComponent>())
                .Select(e => e.GetComponent<EntitySpawnerComponent>());
            foreach (var spawner in spawners)
            {
                await spawner.ProgressSpawner().AwaitCommandFrom(this);
            }


            // Resolve status effects
            await _EnemyTurnService.ResolveEffects().AwaitCommandFrom(this);

            // Refill enemy AP
            _EnemyTurnService.RefillApToMax();

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