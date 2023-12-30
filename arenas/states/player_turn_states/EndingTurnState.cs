using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Widgets;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using ShopIsDone.Arenas.Battles;
using ShopIsDone.Cameras;
using ShopIsDone.Tiles;
using System.Linq;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.PlayerTurn
{
    public partial class EndingTurnState : State
	{
        [Signal]
        public delegate void CanceledEndTurnEventHandler();

        [Signal]
        public delegate void AcceptedEndTurnEventHandler();

        [Export]
        public BattlePhaseManager _PhaseManager;

        [Export]
        private ConfirmEndTurnPopup _ConfirmEndTurnPopup;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        [Inject]
        private PlayerCameraService _PlayerCameraService;

        private Tile _LastSelectedTile;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            InjectionProvider.Inject(this);

            // Hide the cursors
            _TileCursor.Hide();
            _FingerCursor.Hide();

            // Get last selected tile from message to focus cursor on initially
            if (message?.ContainsKey(Consts.LAST_SELECTED_TILE_KEY) ?? false)
            {
                _LastSelectedTile = (Tile)message[Consts.LAST_SELECTED_TILE_KEY];
            }

            // If player has no more moves left, just end the turn immediately
            if (!_PlayerUnitService.HasUnitsThatCanStillAct())
            {
                _PhaseManager.CallDeferred(nameof(_PhaseManager.AdvanceToNextPhase));
                return;
            }

            _PlayerCameraService.Activate();

            // Show a confirmation panel that shows the remaining actions each
            // unit has, mapped to their values
            var endTurnInfo = _PlayerUnitService
                .GetUnits()
                .OrderBy(u => u.EntityName)
                .Select(u =>
                (
                    u.EntityName,
                    _PlayerUnitService
                        .GetUnitRemainingAvailableActions(u)
                        .Select(a => a.MenuTitle)
                        .ToList()
                ))
                .ToList();
            _ConfirmEndTurnPopup.Init(endTurnInfo);
            _ConfirmEndTurnPopup.Show();
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Wait for confirmation / decline input
            if (Input.IsActionJustPressed("ui_accept"))
            {
                EmitSignal(nameof(AcceptedEndTurn));
                _PhaseManager.AdvanceToNextPhase();
                return;
            }

            if (Input.IsActionJustPressed("ui_cancel"))
            {
                EmitSignal(nameof(CanceledEndTurn));
                ChangeState(Consts.States.CHOOSING_UNIT, new Dictionary<string, Variant>()
                {
                    { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
                });
                return;
            }
        }

        public override void OnExit(string nextState)
        {
            // Close the confirmation panel
            _ConfirmEndTurnPopup.Hide();

            // Deactivate camera service
            _PlayerCameraService.Deactivate();

            // Hide the cursors
            _TileCursor.Hide();
            _FingerCursor.Hide();

            base.OnExit(nextState);
        }
    }
}