using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Widgets;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Utils;
using ShopIsDone.Tiles.UI;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Interactables;

namespace ShopIsDone.Arenas.PlayerTurn
{
    // This state represents the player moving their cursor around the board and
    // flipping between selectable entities they want to select or get information on
    public partial class ChoosingUnitState : State
	{
        [Signal]
        public delegate void AttemptedInvalidSelectionEventHandler();

        [Signal]
        public delegate void AttemptedInvalidMoveEventHandler();

        [Signal]
        public delegate void SelectedUnitEventHandler();

        [Signal]
        public delegate void MovedCursorEventHandler();

        [Export]
        public PlayerUnitService _PlayerUnitService;

        [Export]
        private PlayerPawnUIContainer _PawnUIContainer;

        [Export]
        private TileHoverUI _TileHoverUI;

        [Export]
        private InfoUIContainer _InfoUIContainer;

        [Export]
        private Control _EndPlayerTurnWidget;

        [Export]
        private Control _MoreInfoPrompt;

        [Inject]
        private DirectionalInputHelper _InputHelper;

        [Inject]
        private ArenaEntitiesService _EntitiesService;

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private PlayerCameraService _PlayerCameraService;

        [Inject]
        private InputXformer _InputXformer;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ScreenshakeService _Screenshake;

        // Widgets
        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        // State
        private Tile _LastSelectedTile;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Inject dependencies
            InjectionProvider.Inject(this);

            // Get last selected tile from message to focus cursor on initially
            if (message?.ContainsKey(Consts.LAST_SELECTED_TILE_KEY) ?? false)
            {
                _LastSelectedTile = (Tile)message[Consts.LAST_SELECTED_TILE_KEY];
            }
            // Otherwise, just pick the first of the units we have to work with
            else
            {
                var firstUnit = _PlayerUnitService.GetUnitsThatCanStillAct().First();
                _LastSelectedTile = _TileManager.GetTileAtTilemapPos(firstUnit.TilemapPosition);
            }

            // Init tile cursor
            _TileCursor.Init(_TileManager);

            // Have camera follow cursor
            _CameraService.SetCameraTarget(_TileCursor).Execute();

            // Move the cursors to the last selected tile
            _TileCursor.MoveCursorTo(_LastSelectedTile);
            _FingerCursor.WarpCursorTo(_LastSelectedTile.GlobalPosition);

            // Initialize PawnUIContainer with all units in the arena and show it
            _PawnUIContainer.Init(_PlayerUnitService.GetUnits());
            _PawnUIContainer.Show();
            OnCursorHoveredTile(_LastSelectedTile);

            // Show the "End turn" UI
            _EndPlayerTurnWidget.Show();

            // Connect to tile cursor signal and the invalid move signal
            _TileCursor.CursorEnteredTile += OnCursorHoveredTile;
            _TileCursor.AttemptedUnavailableMove += OnAttemptedInvalidMove;

            // Show cursors
            _TileCursor.Show();
            _FingerCursor.Show();

            _PlayerCameraService.Activate();

            // Connect to more info ui signal
            _InfoUIContainer.InfoPanelRequested += OnMoreInfoPayload;

            // Base start hook
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Check for "more info" input
            if (Input.IsActionJustPressed("open_more_info") && _InfoUIContainer.HasCurrentUI())
            {
                // SFX Feedback
                EmitSignal(nameof(SelectedUnit));

                // Request UI
                _InfoUIContainer.RequestInfoPanel();

                return;
            }

            // Check for end turn early input
            if (Input.IsActionJustPressed("end_player_turn"))
            {
                // SFX Feedback
                EmitSignal(nameof(SelectedUnit));

                ChangeState(Consts.States.ENDING_TURN, new Dictionary<string, Variant>()
                {
                    { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
                });
                return;
            }

            // Select unit input
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Get the unit on the tile if they have one
                var unit = _PlayerUnitService
                    .GetUnits()
                    // Only active units
                    .Where(u => u.IsActive())
                    // That have available actions
                    .Where(_PlayerUnitService.UnitHasAvailableActions)
                    .ToList()
                    .Find(e => e.TilemapPosition == _LastSelectedTile.TilemapPosition);

                // If null, emit an invalid selection signal
                if (unit == null)
                {
                    EmitSignal(nameof(AttemptedInvalidSelection));
                    _Screenshake.Shake(
                        ScreenshakeHandler.ShakePayload.ShakeSizes.Tiny,
                        ScreenshakeHandler.ShakeAxis.XOnly
                    );
                    return;
                }

                // Emit a selection signal
                EmitSignal(nameof(SelectedUnit));

                // Then change to the action state
                ChangeState(Consts.States.CHOOSING_ACTION, new Dictionary<string, Variant>()
                {
                    { Consts.SELECTED_UNIT_KEY, unit }
                });
                return;
            }

            // Cycle through units input
            if (Input.IsActionJustPressed("cycle_player_pawns_forward"))
            {
                CycleActivePawns(1);
                return;
            }
            if (Input.IsActionJustPressed("cycle_player_pawns_backward"))
            {
                CycleActivePawns(-1);
                return;
            }

            // Ignore if no movement input
            if (_InputHelper.InputDir == Vector3.Zero) return;

            // Otherwise, move the two cursors in that direction
            _TileCursor.MoveCursorInDirection(_InputHelper.InputDir);
            _FingerCursor.MoveCursorTo(_TileCursor.CurrentTile.GlobalPosition);
        }

        public override void OnExit(string nextState)
        {
            _PlayerCameraService.Deactivate();
            // Remove camera target
            _CameraService.SetCameraTarget(null).Execute();

            // Disconnect from tile cursor
            _TileCursor.CursorEnteredTile -= OnCursorHoveredTile;
            _TileCursor.AttemptedUnavailableMove -= OnAttemptedInvalidMove;

            // Hide the "End turn" UI
            _EndPlayerTurnWidget.Hide();

            // Hide player pawn UI
            _PawnUIContainer.Hide();

            // Hide cursors
            _TileCursor.Hide();
            _FingerCursor.Hide();

            // Clear out any info UI
            _InfoUIContainer.CleanUp();

            // Hide tile UI
            _TileHoverUI.Hide();

            // Hide MoreInfo UI CTA
            _MoreInfoPrompt.Hide();

            // Disconnect from
            _InfoUIContainer.InfoPanelRequested -= OnMoreInfoPayload;

            // Base OnExit
            base.OnExit(nextState);
        }

        private LevelEntity GetActiveUnitOnTile(Tile tile)
        {
            // Get active units that have remaining moves
            var activeUnits = _PlayerUnitService.GetActiveUnits();
            return activeUnits.Contains(tile.UnitOnTile) ? tile.UnitOnTile : null;
        }

        private void OnCursorHoveredTile(Tile tile)
        {
            // Update our internal tile tracking
            _LastSelectedTile = tile;

            // Select the tile to show information about that tile
            SelectTile(_LastSelectedTile);

            // Emit signal
            EmitSignal(nameof(MovedCursor));
        }

        private void SelectTile(Tile tile)
        {
            // Handle tile
            if (!_TileHoverUI.Visible) _TileHoverUI.Show();
            _TileHoverUI.SelectTile(tile);

            // Select Player Pawn UI if there's an active unit on the tile
            _PawnUIContainer.SelectPawnElement(GetActiveUnitOnTile(tile));

            // If the tile is lit, then we can get see an info UI card and opt
            // to see more info about the entities on it
            if (tile.IsLit())
            {
                // Get all hoverable units on the tile
                var entities = _EntitiesService
                    .GetAllArenaEntities()
                    .Where(e => e.IsHoverableOnTile(tile))
                    .Where(_InfoUIContainer.HasTargetableUIForEntity);
                // Create info UI container for whatever gets flagged first on
                // the tile
                // NB: In the future, we might need to be able to flip between
                // the two
                if (entities.Any())
                {
                    _InfoUIContainer.Init(entities.First());
                    _InfoUIContainer.ShowTileInfo();

                    _MoreInfoPrompt.Show();
                }
                // Otherwise, hide it
                else
                {
                    _InfoUIContainer.CleanUp();
                    _MoreInfoPrompt.Hide();
                }
            }
            // Otherwise, hide it all
            else
            {
                _InfoUIContainer.CleanUp();
                _MoreInfoPrompt.Hide();
            }
        }

        private void OnAttemptedInvalidMove()
        {
            EmitSignal(nameof(AttemptedInvalidMove));
            _Screenshake.Shake(
                ScreenshakeHandler.ShakePayload.ShakeSizes.Tiny,
                ScreenshakeHandler.ShakeAxis.XOnly
            );
        }

        // Cycling Pawns and Tasks
        private void CycleActivePawns(int dir)
        {
            // Get all units
            // NB: We can safely assume that there will be at least one to warp to
            var units = _PlayerUnitService.GetUnits();

            // Set up a var for the next selected unit
            LevelEntity newUnit;

            // If we're not on a tile with a unit on it, get the closest unit to
            // the selected tile
            if (!_LastSelectedTile.HasUnitOnTile())
            {
                newUnit = units
                    .OrderBy(pawn => _LastSelectedTile.TilemapPosition.DistanceTo(pawn.TilemapPosition))
                    .FirstOrDefault();
            }
            // Otherwise, out of the list of active units, go either forward or backwards in
            // the list based on our currently selected unit
            else
            {
                var currentUnit = _LastSelectedTile.UnitOnTile;
                var selectedIdx = units.ToList().IndexOf(currentUnit);

                // Select circularly
                newUnit = units.SelectCircular(selectedIdx, dir);
            }

            // Warp the cursors to the new tile
            var newUnitTile = _TileManager.GetTileAtTilemapPos(newUnit.TilemapPosition);
            _TileCursor.MoveCursorTo(newUnitTile);
            _FingerCursor.WarpCursorTo(_TileCursor.CurrentTile.GlobalPosition);
        }

        private void OnMoreInfoPayload(MoreInfoPayload payload)
        {
            // Change to "More Info" state with payload
            ChangeState(Consts.States.MORE_INFO, new Dictionary<string, Variant>()
            {
                { Consts.MORE_INFO_PAYLOAD_KEY, payload },
                { Consts.LAST_SELECTED_TILE_KEY, _LastSelectedTile }
            });
        }
    }
}
