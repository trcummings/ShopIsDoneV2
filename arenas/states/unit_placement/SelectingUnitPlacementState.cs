using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Arenas.States;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Tiles;
using ShopIsDone.Levels;
using System.Linq;

namespace ShopIsDone.Arenas.UnitPlacement
{
	public partial class SelectingUnitPlacementState : State
	{
		[Export]
		private InitializingArenaState _InitState;

        [Export]
        private TileManager _TileManager;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;

        public override void _Ready()
		{

		}

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Get player units
            var allUnits = _PlayerCharacterManager.GetAllUnits();

            // Get placement tiles
            var placementTiles = _TileManager
                .GetPlacementTiles()
                .Take(allUnits.Count)
                .ToArray();

            // Place the player units on the first few placement tiles we have
            for (int i = 0; i < placementTiles.Length; i++)
            {
                var placement = placementTiles[i];
                var unit = allUnits[i];
                unit.GlobalPosition = placement.GlobalPosition;
            }

            // Finish immediately
            _InitState.Finish();
        }
    }
}