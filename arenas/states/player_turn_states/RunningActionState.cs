using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Widgets;
using ShopIsDone.Actions;
using ShopIsDone.Utils.DependencyInjection;
using ActionConsts = ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Consts;
using ShopIsDone.Tiles;
using System.Linq;

namespace ShopIsDone.Arenas.PlayerTurn
{
	public partial class RunningActionState : State
	{
        [Inject]
        private ActionService _ActionService;

        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private PlayerUnitService _PlayerUnitService;

        public override async void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            InjectionProvider.Inject(this);

            // Pull the unit and action and payload from the parameters
            var action = (ArenaAction)message[ActionConsts.ACTION_KEY];
            var unit = action.Entity;

            // Track the unit's current tile
            var unitLastTile = _TileManager.GetTileAtTilemapPos(unit.TilemapPosition);

            // Hide all UI and the cursors
            _FingerCursor.Hide();
            _TileCursor.Hide();

            // Execute action through PTM
            var command = _ActionService.ExecuteAction(action, message);
            command.CallDeferred(nameof(command.Execute));
            await ToSignal(command, "Finished");

            // Show the cursors
            _FingerCursor.Show();
            _TileCursor.Show();

            // If unit can still act, continue choosing actions for that unit
            if (unit.GetComponent<ActionHandler>().HasAvailableActions())
            {
                // Focus cursors on unit
                var newTile = _TileManager.GetTileAtTilemapPos(unit.TilemapPosition);
                _TileCursor.MoveCursorTo(newTile);
                _FingerCursor.WarpCursorTo(newTile.GlobalPosition);

                // Go back to ChoosingActionState
                ChangeState(Consts.States.CHOOSING_ACTION, new Dictionary<string, Variant>()
                {
                    { Consts.SELECTED_UNIT_KEY, unit }
                });
            }
            // If not only this unit can act, but no units remain to act, then end
            // the turn immediately (this is the prelude to a player victory)
            else if (!_PlayerUnitService.PlayerUnits.Any(u => u.IsInArena()))
            {
                ChangeState(Consts.States.ENDING_TURN);
            }
            // Otherwise, go back to choosing a unit in the ChoosingUnitState
            else
            {
                ChangeState(Consts.States.CHOOSING_UNIT, new Dictionary<string, Variant>()
                {
                    { Consts.LAST_SELECTED_TILE_KEY, unitLastTile }
                });
            }
        }
    }
}