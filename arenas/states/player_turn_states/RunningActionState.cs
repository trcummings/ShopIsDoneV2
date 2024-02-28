using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Utils.DependencyInjection;
using ActionConsts = ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Consts;
using ShopIsDone.Tiles;

namespace ShopIsDone.Arenas.PlayerTurn
{
	public partial class RunningActionState : State
	{
        [Export]
        private ActionService _ActionService;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ArenaOutcomeService _OutcomeService;

        public override async void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            InjectionProvider.Inject(this);

            // Pull the unit and action and payload from the parameters
            var action = (ArenaAction)message[ActionConsts.ACTION_KEY];
            var unit = action.Entity;

            // Track the unit's current tile
            var unitLastTile = _TileManager.GetTileAtTilemapPos(unit.TilemapPosition);

            // Execute action
            var command = _ActionService.ExecuteAction(action, message);
            command.CallDeferred(nameof(command.Execute));
            await ToSignal(command, nameof(command.Finished));

            // If an outcome was reached, do not continue any of this
            if (_OutcomeService.WasOutcomeReached()) return;

            // If unit can still act, continue choosing actions for that unit
            if (_PlayerUnitService.UnitHasAvailableActions(unit))
            {
                // Go back to choosing actions
                ChangeState(Consts.States.CHOOSING_ACTION, new Dictionary<string, Variant>()
                {
                    { Consts.SELECTED_UNIT_KEY, unit }
                });
            }
            // If not only this unit can't act, but no units remain to act, then end
            // the turn immediately (this is the prelude to a player victory)
            else if (!_PlayerUnitService.HasUnitsThatCanStillAct())
            {
                ChangeState(Consts.States.ENDING_TURN, new Dictionary<string, Variant>()
                {
                    { Consts.LAST_SELECTED_TILE_KEY, unitLastTile }
                });
            }
            // Otherwise, go back to choosing the next unit
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