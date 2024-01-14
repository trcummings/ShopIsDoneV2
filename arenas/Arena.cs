using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Actions;
using ShopIsDone.Actions.Effort;
using ShopIsDone.Arenas.ArenaScripts;
using ArenaConsts = ShopIsDone.Arenas.States.Consts;
using ShopIsDone.Conditions;
using ShopIsDone.Levels;
using ShopIsDone.ArenaInteractions;
using ShopIsDone.Arenas.Meddling;
using ShopIsDone.Tasks;
using ShopIsDone.Entities.ParallelHunters;

namespace ShopIsDone.Arenas
{
    // The purpose of the Arena is to register Arena-specific Services with the
    // Injection Container, and to clean up when the Arena is finished
    public partial class Arena : Node3D
    {
        [Signal]
        public delegate void ArenaFinishedEventHandler();

        [Signal]
        public delegate void ArenaFailedEventHandler();

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
        private TaskService _TaskService;

        [Export]
        private EffortMeterService _EffortMeterService;

        [Export]
        private ScriptQueueService _ScriptQueueService;

        [Export]
        private ArenaOutcomeService _OutcomeService;

        [Export]
        private UnitDeathService _UnitDeathService;

        [Export]
        private ConditionsService _ConditionsService;

        [Export]
        private UnitInteractionService _UnitInteractionService;

        [Export]
        private UnitTaskService _UnitTaskService;

        // Not services
        [Export]
        private ActionMeddler _ActionMeddler;

        [Export]
        private ParallelHunterService _ParallelHunterService;

        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public async void Init()
        {
            // Register services
            _InjectionProvider.RegisterService(_TileManager);
            _InjectionProvider.RegisterService(_ActionService);
            _InjectionProvider.RegisterService(_PlayerUnitService);
            _InjectionProvider.RegisterService(_TaskService);
            _InjectionProvider.RegisterService(_EffortMeterService);
            _InjectionProvider.RegisterService(_ScriptQueueService);
            _InjectionProvider.RegisterService(_OutcomeService);
            _InjectionProvider.RegisterService(_UnitDeathService);
            _InjectionProvider.RegisterService(_ConditionsService);
            _InjectionProvider.RegisterService(_UnitInteractionService);
            _InjectionProvider.RegisterService(_UnitTaskService);

            // Initialize services (with strict order)
            _TileManager.Init();
            _ActionService.Init();
            _TaskService.Init();
            _UnitDeathService.Init();
            _ConditionsService.Init();
            _ActionMeddler.Init();
            _ParallelHunterService.Init();

            // Move units into arena
            _PlayerCharacterManager.EnterArena();
            var allUnits = _PlayerCharacterManager.GetAllUnits();
            var placementTiles = GetTree()
                .GetNodesInGroup("placement_tile")
                .OfType<Tile>()
                .Where(IsAncestorOf)
                .Take(allUnits.Count)
                .ToArray();
            for (int i = 0; i < placementTiles.Count(); i++)
            {
                var placement = placementTiles[i];
                var unit = allUnits[i];
                unit.GlobalPosition = placement.GlobalPosition;
            }

            // Add player units to player unit service
            _PlayerUnitService.Init(allUnits.ToList<LevelEntity>());

            // Update all tiles
            _TileManager.UpdateTiles();

            await ToSignal(GetTree(), "process_frame");

            // Init all entities under the arena
            var allEntities = GetTree()
                .GetNodesInGroup("entities")
                .OfType<LevelEntity>()
                .Where(IsAncestorOf);
            foreach (var entity in allEntities) entity.Init();

            // Update all tiles
            _TileManager.UpdateTiles();

            // Change state to initializing
            _ArenaStateMachine.ChangeState(ArenaConsts.INITIALIZING);
        }

        public void CleanUp()
        {
            // Unregister services
            _InjectionProvider.UnregisterService(_TileManager);
            _InjectionProvider.UnregisterService(_ActionService);
            _InjectionProvider.UnregisterService(_PlayerUnitService);
            _InjectionProvider.UnregisterService(_TaskService);
            _InjectionProvider.UnregisterService(_EffortMeterService);
            _InjectionProvider.UnregisterService(_ScriptQueueService);
            _InjectionProvider.UnregisterService(_OutcomeService);
            _InjectionProvider.UnregisterService(_UnitDeathService);
            _InjectionProvider.UnregisterService(_ConditionsService);
            _InjectionProvider.UnregisterService(_UnitInteractionService);
            _InjectionProvider.UnregisterService(_UnitTaskService);

            // Idle player units
            _PlayerCharacterManager.Idle();

            // Clean up tile manager tiles
            _TileManager.CleanUp();
        }

        public void FinishArena()
        {
            EmitSignal(nameof(ArenaFinished));
        }

        public void FailArena()
        {
            EmitSignal(nameof(ArenaFailed));
        }
    }
}

