using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Arenas.UI;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.DependencyInjection;
using ActionConsts = ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Consts;
using ShopIsDone.Arenas.PlayerTurn.ChoosingActions;
using System.Linq;
using ShopIsDone.Widgets;

namespace ShopIsDone.Arenas.PlayerTurn
{
	public partial class ChoosingActionState : State
	{
        [Export]
        public StateMachine _ActionStateMachine;

        [Export]
        public PlayerPawnUI _PlayerPawnUI;

        [Inject]
        private TileManager _TileManager;

        // Widgets
        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        // State Variables
        private LevelEntity _SelectedUnit;

        public override void _Ready()
        {
            base._Ready();

            // Idle the action state machine
            _ActionStateMachine.ChangeState(ActionConsts.States.IDLE);
        }

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            InjectionProvider.Inject(this);

            // Connect to state signals
            foreach (var state in _ActionStateMachine.States.OfType<ActionState>())
            {
                state.GoBackRequested += GoBackToChoosingUnit;
                state.MainMenuRequested += GoToMainActionMenu;
                state.RunActionRequested += RunAction;
                state.PawnAPDiffRequested += _PlayerPawnUI.SetApDiff;
                state.PawnAPDiffCanceled += _PlayerPawnUI.ClearApDiff;
            }

            // Get selected Unit from message
            _SelectedUnit = (LevelEntity)message[Consts.SELECTED_UNIT_KEY];

            // Show the selected player pawn UI
            _PlayerPawnUI.Init(_SelectedUnit);
            _PlayerPawnUI.SelectPawnUI(true);
            _PlayerPawnUI.Show();

            // Show cursors at the selected unit
            var unitTile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);
            _TileCursor.MoveCursorTo(unitTile);
            _FingerCursor.WarpCursorTo(unitTile.GlobalPosition);
            // Show after a frame delay so there isn't an unpleasant warping effect
            GetTree().Connect(
                "process_frame",
                Callable.From(() =>
                {
                    _TileCursor.Show();
                    _FingerCursor.Show();
                }),
                (uint)ConnectFlags.OneShot
            );

            // base start
            base.OnStart(message);

            // Go right to the main action menu
            GoToMainActionMenu();
        }

        public override void OnExit(string nextState)
        {
            // Set the action state machine back to idle
            _ActionStateMachine.ChangeState(ActionConsts.States.IDLE);

            // Disconnect from state signals
            foreach (var state in _ActionStateMachine.States.OfType<ActionState>())
            {
                state.GoBackRequested -= GoBackToChoosingUnit;
                state.MainMenuRequested -= GoToMainActionMenu;
                state.RunActionRequested -= RunAction;
                state.PawnAPDiffRequested -= _PlayerPawnUI.SetApDiff;
                state.PawnAPDiffCanceled -= _PlayerPawnUI.ClearApDiff;
            }

            // Hide the Player Pawn UI
            _PlayerPawnUI.Hide();

            // Hide cursors
            _TileCursor.Hide();
            _FingerCursor.Hide();

            base.OnExit(nextState);
        }

        // This function is public for our child states to use
        private void GoToMainActionMenu()
        {
            _ActionStateMachine.ChangeState(ActionConsts.States.MENU, new Dictionary<string, Variant>()
            {
                // Pass through selected unit
                { Consts.SELECTED_UNIT_KEY, _SelectedUnit },
            });
        }

        private void GoBackToChoosingUnit()
        {
            // Change state back to choosing unit state with the last selected
            // tile of the unit we were on
            var tile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);
            ChangeState(Consts.States.CHOOSING_UNIT, new Dictionary<string, Variant>()
            {
                { Consts.LAST_SELECTED_TILE_KEY, tile }
            });
        }

        private void RunAction(Dictionary<string, Variant> message = null)
        {
            // Change state to running action state with the proper payload
            ChangeState(Consts.States.RUNNING_ACTION, message);
        }
    }
}