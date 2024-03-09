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
using ShopIsDone.Utils.Extensions;

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

        // State
        private Array<LevelEntity> _Units = new Array<LevelEntity>();


        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Disable Pausing
            _PauseInputHandler.IsActive = false;

            // Get player units
            _Units = _PlayerCharacterManager
                .GetAllUnits()
                .OfType<LevelEntity>()
                .ToGodotArray();

            // TODO: Skip placement if we have some sort of save data for the
            // level

            // Get placement tiles
            var placementTiles = _TileManager
                .GetPlacementTiles()
                .Take(_Units.Count)
                .ToArray();

            // Place the player units on the first few placement tiles we have
            for (int i = 0; i < placementTiles.Length; i++)
            {
                var placement = placementTiles[i];
                var unit = _Units[i];
                unit.GlobalPosition = placement.GlobalPosition;
                // Set the unit's facing direction to the placement tile's direction
                unit.FacingDirection = placement.PlacementFacingDir;
            }

            // Add player units to player unit service
            _PlayerUnitService.Init(_Units.ToList());

            // Change to the selecting unit state
            _PlacementStateMachine.ChangeState(PlacementConsts.States.SELECTING_UNIT);
        }

        public async void Finish()
        {
            // Change placement state machine to idle
            _PlacementStateMachine.ChangeState(PlacementConsts.States.IDLE);

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
