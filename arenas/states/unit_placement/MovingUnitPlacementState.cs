using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using ShopIsDone.Utils;
using ShopIsDone.Cameras;
using Godot.Collections;
using ShopIsDone.Tiles;
using ShopIsDone.Core;
using System.Linq;

namespace ShopIsDone.Arenas.UnitPlacement
{
	public partial class MovingUnitPlacementState : State
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
        private Control _CancelPrompt;

        [Export]
        private Control _SwapPrompt;

        [Export]
        private Control _PlacePrompt;

        [Inject]
        private ScreenshakeService _Screenshake;

        // Widgets
        [Inject]
        private DirectionalInputHelper _InputHelper;

        [Inject]
        private TileCursor _TileCursor;

        // State
        private Tile _SelectedTile = null;
        private Tile _InitialSelectedTile = null;
        private LevelEntity _SelectedUnit = null;
        private Array<LevelEntity> _Units = new Array<LevelEntity>();
        private Dictionary<Vector3, Tile> _PlacementTiles = new Dictionary<Vector3, Tile>();


        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Inject dependencies
            InjectionProvider.Inject(this);

            // Pull values from the message
            _InitialSelectedTile = (Tile)message[Consts.INITIAL_TILE_KEY];
            _SelectedUnit = (LevelEntity)message[Consts.SELECTED_UNIT_KEY];
            _Units = (Array<LevelEntity>)message[Consts.UNITS_KEY];
            _PlacementTiles = (Dictionary<Vector3, Tile>)message[Consts.TILES_KEY];

            // Initially set selected tile as the initial tile
            _SelectedTile = _InitialSelectedTile;

            // Initially show cancel prompt
            _CancelPrompt.Show();

            base.OnStart(message);
        }

        public override void OnExit(string nextState)
        {
            // Hide prompts
            _CancelPrompt.Hide();
            _SwapPrompt.Hide();
            _PlacePrompt.Hide();

            base.OnExit(nextState);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Cancel and reset
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Emit
                EmitSignal(nameof(Canceled));

                // Save if we're on a new tile or not
                var wasOnInitialTile = _InitialSelectedTile == _SelectedTile;

                // Revert to the initial tile
                MoveToTile(_InitialSelectedTile);

                // Go back to selecting state if we were on the initial tile
                if (wasOnInitialTile)
                {
                    ChangeState(Consts.States.SELECTING_UNIT, new Dictionary<string, Variant>()
                {
                        { Consts.SELECTED_UNIT_KEY, _SelectedUnit }
                    });
                }

                // Return early
                return;
            }

            // Select unit input
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Get the new unit
                var newUnit = _Units
                    .ToList()
                    .Find(u => u.TilemapPosition == _SelectedTile.TilemapPosition);

                // If it's the same unit, emit invalid
                if (_SelectedUnit == newUnit)
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
                // If the tile is not empty, then swap the units places
                else if (newUnit != null)
                {
                    // Persist the positions
                    var newUnitPos = newUnit.GlobalPosition;
                    var selectedUnitPos = _SelectedUnit.GlobalPosition;
                    // Swap them
                    newUnit.GlobalPosition = selectedUnitPos;
                    _SelectedUnit.GlobalPosition = newUnitPos;
                }
                // Otherwise, just move the unit onto the tile
                else
                {
                    _SelectedUnit.GlobalPosition = _SelectedTile.GlobalPosition;
                }

                // Emit a selection signal
                EmitSignal(nameof(Confirmed));

                // Then go back to selecting state
                ChangeState(Consts.States.SELECTING_UNIT, new Dictionary<string, Variant>()
                {
                    { Consts.SELECTED_UNIT_KEY, _SelectedUnit }
                });

                // Return early
                return;
            }

            // Ignore if no movement input
            if (_InputHelper.JustPressedInputDir == Vector3.Zero) return;

            // Check for invalid movement
            var newTile = _SelectedTile.TilemapPosition + _InputHelper.JustPressedInputDir;
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

            // Emit
            EmitSignal(nameof(Selected));

            // Move to the new tile
            MoveToTile(_PlacementTiles[newTile]);
        }

        private void MoveToTile(Tile tile)
        {
            // Set the selected tile
            _SelectedTile = tile;

            // Move the finger cursor
            _TileCursor.MoveCursorTo(tile);

            // Get the new unit
            var newUnit = _Units
                .ToList()
                .Find(u => u.TilemapPosition == _SelectedTile.TilemapPosition);

            // If it's the initial tile, hide the move prompts
            if (_SelectedUnit == newUnit)
            {
                _PlacePrompt.Hide();
                _SwapPrompt.Hide();
            }
            // If the tile is not empty, show the swap prompt
            else if (newUnit != null)
            {
                _PlacePrompt.Hide();
                _SwapPrompt.Show();
            }
            // Otherwise, just move the unit onto the tile
            else
            {
                _PlacePrompt.Show();
                _SwapPrompt.Hide();
            }
        }
    }
}
