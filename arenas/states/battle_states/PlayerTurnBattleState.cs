using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Tiles;
using Consts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.Utils.DependencyInjection;
using System.Linq;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class PlayerTurnBattleState : State
	{
        [Export]
        private StateMachine _PlayerTurnStateMachine;

        [Export]
        public PlayerUnitService _PlayerUnitService;

        [Inject]
        private TileManager _TileManager;

        private Tile _LastSelectedTile = null;

        public override void _Ready()
        {
            base._Ready();

            // Initialize battle state machine
            _PlayerTurnStateMachine.ChangeState(Consts.States.IDLE);
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Dependency injection
            InjectionProvider.Inject(this);

            // Save temp arena data at start of turn
            //SaveTempArenaData();

            // Get all units
            var units = _PlayerUnitService.GetUnits();

            // If we have no active units, end the turn
            if (units.Count == 0)
            {
                _PlayerTurnStateMachine.ChangeState(Consts.States.ENDING_TURN);
                return;
            }

            // If no "last selected tile" (from last turn), pick the tile where
            // the last unit the player the player made move, even if that unit
            // is now gone. If it's null, just grab the first unit in the list
            // and focus that tile
            if (_LastSelectedTile == null)
            {
                var firstUnit = units.First();
                _LastSelectedTile = _TileManager.GetTileAtTilemapPos(firstUnit.TilemapPosition);
            }

            // Initialize and pass along the last selected tile
            _PlayerTurnStateMachine.ChangeState(Consts.States.CHOOSING_UNIT, new Dictionary<string, Variant>()
            {
                { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
            });
        }
    }
}
