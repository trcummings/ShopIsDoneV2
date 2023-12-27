using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Tiles;
using Consts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.Battles.States
{
	public partial class PlayerTurnBattleState : State
	{
        [Export]
        private StateMachine _StateMachine;

        [Inject]
        private TileManager _TileManager;

        public Tile LastSelectedTile
        { get { return _LastSelectedTile; } }
        private Tile _LastSelectedTile = null;

        public override void _Ready()
        {
            base._Ready();
            // Initialize state machine
            _StateMachine.ChangeState("Init");
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Dependency injection
            InjectionProvider.Inject(this);

            // Save temp arena data at start of turn
            //SaveTempArenaData();

            // Get all units
            //var units = GetActivePlayerPawns();

            //// If we have no active units, end the turn
            //if (units.Count() == 0)
            //{
            //    ChangeState("EndingTurn");
            //    return;
            //}

            // If no "last selected tile", pick the tile where the last unit the player
            // the player made move, even if that unit is now gone. If it's null,
            // just grab the first unit in the list and focus that tile
            if (_LastSelectedTile == null)
            {
                //var firstUnit = GetActivePlayerPawns().First();
                //LastSelectedTile = _TileManager.GetTileAtTilemapPos(firstUnit.TilemapPosition);
            }

            // Initialize and pass along the last selected tile
            _StateMachine.ChangeState(Consts.States.CHOOSING_UNIT_STATE, new Dictionary<string, Variant>()
            {
                { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
            });
        }
    }
}
