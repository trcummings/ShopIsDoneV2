using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Core;
using ShopIsDone.Levels;
using ShopIsDone.Tiles;
using System.Linq;
using PlacementConsts = ShopIsDone.Arenas.UnitPlacement.Consts;

namespace ShopIsDone.Arenas.States
{
	public partial class InitializingArenaState : State
	{
        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private ArenaEntitiesService _EntitiesService;

        [Export]
        private TileManager _TileManager;

        [Export]
        private StateMachine _PlacementStateMachine;

        [Inject]
        private PauseInputHandler _PauseInputHandler;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;


        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Disable Pausing
            _PauseInputHandler.IsActive = false;

            // TODO: Skip placement if we have some sort of save data for the
            // level
            _PlacementStateMachine.ChangeState(PlacementConsts.SELECTING_UNIT);
        }

        public async void Finish()
        {
            // Get player units
            var allUnits = _PlayerCharacterManager.GetAllUnits();

            // Add player units to player unit service
            _PlayerUnitService.Init(allUnits.ToList<LevelEntity>());

            // Update all tiles
            _TileManager.UpdateTiles();

            await ToSignal(GetTree(), "process_frame");

            // Init all entities under the arena
            foreach (var entity in _EntitiesService.GetArenaChildEntities())
            {
                entity.Init();
            }

            // Update all tiles
            _TileManager.UpdateTiles();

            await ToSignal(GetTree(), "process_frame");

            // Go directly to running state
            ChangeState(Consts.RUNNING);
        }
    }
}
