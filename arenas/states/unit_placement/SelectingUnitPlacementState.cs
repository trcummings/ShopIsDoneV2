using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Arenas.States;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Tiles;
using ShopIsDone.Levels;
using System.Linq;
using ShopIsDone.Widgets;
using ShopIsDone.Utils;
using ShopIsDone.Arenas.UnitPlacement.UI;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.UnitPlacement
{
	public partial class SelectingUnitPlacementState : State
	{
        [Signal]
        public delegate void InvalidEventHandler();

        [Signal]
        public delegate void CanceledEventHandler();

        [Signal]
        public delegate void SelectedEventHandler();

        [Signal]
        public delegate void ConfirmedEventHandler();

        [Export]
		private InitializingArenaState _InitState;

        [Export]
        private TileManager _TileManager;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Inject]
        private DirectionalInputHelper _InputHelper;

        [Inject]
        private ScreenshakeService _Screenshake;

        [Inject]
        private CameraService _CameraService;

        // Widgets
        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        [Inject]
        private TileIndicator _TileIndicator;

        [Export]
        private HeldInputHelper _ConfirmPlacementInput;

        [Export]
        private ConfirmPlacementPrompt _ConfirmPrompt;

        [Export]
        private Control _SelectPrompt;

        [Export]
        private float _HoldConfirmTime = 1.5f;

        // State
        private Tile _SelectedTile = null;
        private Array<LevelEntity> _Units = new Array<LevelEntity>();
        private Dictionary<Vector3, Tile> _PlacementTiles = new Dictionary<Vector3, Tile>();

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Update tiles
            _TileManager.UpdateTiles();

            // Get player units
            _Units = _PlayerCharacterManager
                .GetAllUnits()
                .OfType<LevelEntity>()
                .ToGodotArray();

            // Set the placement tiles dictionary
            _PlacementTiles = _TileManager
                .GetPlacementTiles()
                .Aggregate(new Dictionary<Vector3, Tile>(), (acc, tile) =>
                {
                    acc.Add(tile.TilemapPosition, tile);
                    return acc;
                });

            // Create indicators for the tiles
            _TileIndicator.ClearIndicators();
            _TileIndicator.CreateIndicators(
                _PlacementTiles.Values.Select(tile => tile.GlobalPosition),
                TileIndicator.IndicatorColor.Purple
            );

            // Pull out selected unit from message
            var selectedUnit =
                (LevelEntity)message?[Consts.SELECTED_UNIT_KEY] ??
                // Default to the leader
                _PlayerCharacterManager.Leader;

            // Place cursor on leader
            var selectedTile = _TileManager.GetTileAtGlobalPos(selectedUnit.GlobalPosition);

            // Set selected tile
            _SelectedTile = selectedTile;

            // Init tile cursor
            _TileCursor.Init(_TileManager);

            // Move the cursors to the last selected tile
            _TileCursor.MoveCursorTo(_SelectedTile);
            _FingerCursor.WarpCursorTo(_SelectedTile.GlobalPosition);

            // Have the camera follow the finger cursor
            _CameraService.SetCameraTarget(_FingerCursor).Execute();

            // Rotate the camera so it's facing the selected unit
            _CameraService.RotateCameraTo(selectedUnit.FacingDirection);

            // Show cursors
            _FingerCursor.Show();
            _TileCursor.Show();

            // Show confirm prompt
            _ConfirmPrompt.Show();

            // Show select prompt
            _SelectPrompt.Show();
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Handle confirmation press
            var wasHeldEnough = _ConfirmPlacementInput.WasHeldFor(_HoldConfirmTime);

            // Update radial progress
            var progress = _ConfirmPlacementInput.HeldTime / _HoldConfirmTime * 100;
            _ConfirmPrompt.SetRadialProgress(progress);

            // If we've been held long enough, finish immediately
            if (wasHeldEnough) _InitState.Finish();

            // Select unit input
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Get the unit on the tile if they have one
                var unitOnTile = _Units
                    .ToList()
                    .Find(u => u.TilemapPosition == _SelectedTile.TilemapPosition);

                // If null, emit an invalid selection signal
                if (unitOnTile == null)
                {
                    EmitSignal(nameof(Invalid));
                    _Screenshake.Shake(
                        ScreenshakeHandler.ShakePayload.ShakeSizes.Tiny,
                        ScreenshakeHandler.ShakeAxis.XOnly
                    );
                    return;
                }

                // Emit a selection signal
                EmitSignal(nameof(Confirmed));

                // Then change to the moving unit state
                ChangeState(Consts.States.MOVING_UNIT, new Dictionary<string, Variant>()
                {
                    { Consts.SELECTED_UNIT_KEY, unitOnTile },
                    { Consts.INITIAL_TILE_KEY, _SelectedTile },
                    { Consts.UNITS_KEY, _Units },
                    { Consts.TILES_KEY, _PlacementTiles },
                });

                // Return early
                return;
            }

            // Ignore if no movement input
            if (_InputHelper.InputDir == Vector3.Zero) return;

            // Check for invalid movement
            var newTile = _SelectedTile.TilemapPosition + _InputHelper.InputDir;
            if (!_PlacementTiles.ContainsKey(newTile))
            {
                // Emit
                EmitSignal(nameof(Invalid));

                // Screenshake
                _Screenshake.Shake(
                    ScreenshakeHandler.ShakePayload.ShakeSizes.Tiny,
                    ScreenshakeHandler.ShakeAxis.XOnly
                );

                // Return early
                return;
            }

            // Otherwise, move the cursors to that position
            _TileCursor.MoveCursorInDirection(_InputHelper.InputDir);
            _FingerCursor.MoveCursorTo(_TileCursor.CurrentTile.GlobalPosition);

            // Set the selected tile
            _SelectedTile = _PlacementTiles[newTile];

            // Get the unit on the tile if they have one
            var unit = _Units
                .ToList()
                .Find(u => u.TilemapPosition == _SelectedTile.TilemapPosition);

            // If there's no unit on the tile, then hide the select prompt
            if (unit == null) _SelectPrompt.Hide();
            // Otherwise show it
            else _SelectPrompt.Show();

            // Emit
            EmitSignal(nameof(Selected));
        }

        public override void OnExit(string nextState)
        {
            // Hide confirm prompt
            _ConfirmPrompt.Hide();

            // Hide select prompt
            _SelectPrompt.Hide();

            // If the next state isn't the moving state, clear it all out
            if (nextState != Consts.States.MOVING_UNIT)
            {
                // Remove camera target
                _CameraService.SetCameraTarget(null).Execute();

                // Clear indicators
                _TileIndicator.ClearIndicators();

                // Hide cursors
                _FingerCursor.Hide();
                _TileCursor.Hide();
            }

            base.OnExit(nextState);
        }
    }
}