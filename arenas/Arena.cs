using Godot;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using System.Linq;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;
using ArenaConsts = ShopIsDone.Arenas.States.Consts;
using ShopIsDone.Levels;

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

        [Export]
        private TileManager _TileManager;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        // NB: Technically a service, but it lives under Level
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        // Services
        private ServicesContainer _Services;

        public override void _Ready()
        {
            base._Ready();
            _Services = GetNode<ServicesContainer>("%Services");
        }

        public async void Init()
        {
            _Services.RegisterServices();
            _Services.InitServices();

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
            _Services.UnregisterServices();

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

