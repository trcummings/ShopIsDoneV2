using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Actions;
using ShopIsDone.Actions.Effort;
using ShopIsDone.Arenas.ArenaScripts;

namespace ShopIsDone.Arenas
{
    // The purpose of the Arena is to register Arena-specific Services with the
    // Injection Container, and to clean up when the Arena is finished
    public partial class Arena : Node3D
    {
        [Export]
        public StateMachine _ArenaStateMachine;

        // Services
        [Export]
        private TileManager _TileManager;

        [Export]
        private ActionService _ActionService;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private EffortMeterService _EffortMeterService;

        [Export]
        private ScriptQueueService _ScriptQueueService;

        [Export]
        private ArenaOutcomeService _OutcomeService;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public void Init(LevelEntity playerUnit)
        {
            // Register services
            _InjectionProvider.RegisterService(_TileManager);
            _InjectionProvider.RegisterService(_ActionService);
            _InjectionProvider.RegisterService(_PlayerUnitService);
            _InjectionProvider.RegisterService(_EffortMeterService);
            _InjectionProvider.RegisterService(_ScriptQueueService);
            _InjectionProvider.RegisterService(_OutcomeService);

            // Initialize services (with strict order)
            _TileManager.Init();
            _ActionService.Init();

            // Move units into arena
            var placementTiles = GetTree()
                .GetNodesInGroup("placement_tile")
                .OfType<Tile>()
                .Where(IsAncestorOf);
            var placement = placementTiles.First();
            playerUnit.GlobalPosition = placement.GlobalPosition;

            // Add player unit to player unit service
            _PlayerUnitService.Init(new System.Collections.Generic.List<LevelEntity>() { playerUnit });
            // Init all entities
            var allEntities = GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                .Where(IsAncestorOf);
            foreach (var entity in allEntities) entity.Init();

            // Update all tiles
            _TileManager.UpdateTiles();

            // Change state to initializing
            _ArenaStateMachine.ChangeState("Initializing");
        }

        public void CleanUp()
        {
            // Unregister services
            _InjectionProvider.UnregisterService(_TileManager);
            _InjectionProvider.UnregisterService(_ActionService);
            _InjectionProvider.UnregisterService(_PlayerUnitService);
            _InjectionProvider.UnregisterService(_EffortMeterService);
            _InjectionProvider.UnregisterService(_ScriptQueueService);
            _InjectionProvider.UnregisterService(_OutcomeService);
        }
    }
}

