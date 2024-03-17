using Godot;
using System;
using ShopIsDone.Tiles;
using ShopIsDone.Utils;
using ShopIsDone.Utils.Pathfinding;
using ShopIsDone.Widgets;
using static ShopIsDone.Widgets.TileIndicator;
using Godot.Collections;
using GenericCollections = System.Collections.Generic;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using System.Linq;
using ShopIsDone.ActionPoints;
using ActionConsts = ShopIsDone.Actions.Consts;
using ShopIsDone.Utils.Positioning;
using ShopIsDone.Cameras;
using ShopIsDone.Arenas.UI;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    // This state deals with targeting a unit for some kind of action
	public partial class TargetingState : ActionState
    {
        [Signal]
        public delegate void ChangedSelectionEventHandler();

        [Signal]
        public delegate void ConfirmedSelectionEventHandler();

        [Signal]
        public delegate void ConfirmedInvalidSelectionEventHandler();

        [Signal]
        public delegate void CanceledSelectionEventHandler();

        // Nodes
        [Export]
        private InfoUIContainer _InfoUIContainer;

        [Inject]
        private DirectionalInputHelper _DirectionalInputHelper;

        [Inject]
        private TileIndicator _TileIndicatorWidget;

        [Inject]
        private TileCursor _TileCursor;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ScreenshakeService _Screenshake;

        // State Variables
        private Vector3 _InitialUnitFacingDir = Vector3.Zero;
        private Tile _InitialCursorTile;
        private GenericCollections.List<Tile> _AvailableTiles = new GenericCollections.List<Tile>();

        public override void _Ready()
        {
            base._Ready();

            ConfirmedInvalidSelection += () =>
            {
                // A little bit of screenshake just on the x-axis on invalid
                _Screenshake.Shake(
                    ScreenshakeHandler.ShakePayload.ShakeSizes.Mild,
                    ScreenshakeHandler.ShakeAxis.XOnly
                );
            };
        }

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);
            InjectionProvider.Inject(this);

            // Persist its initial facing direction
            _InitialUnitFacingDir = _SelectedUnit.FacingDirection;

            // Grab the allowed tile range from the message
            var range = _Action.Range;

            // Grab if we can target self from the message
            var canTargetSelf = _Action.CanTargetSelf;

            // Choose the origin for the tile selection
            var originTile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);

            // Get the allowed tiles based on the range
            _AvailableTiles = new MoveGenerator()
                .GetAvailableMoves(originTile, canTargetSelf, range)
                // Filter tiles out with obstacles on them or lit tiles
                .Where(tile => !tile.HasObstacleOnTile)
                // Filter out tiles that aren't visible but are more than 1
                // square away (we can always target stuff right next to us)
                .Where(tile => !(
                    tile.TilemapPosition.DistanceTo(_SelectedUnit.TilemapPosition) > 1 &&
                    !tile.IsLit()
                ))
                .ToList();

            // Perist the tile cursor's initial position
            _InitialCursorTile = _TileCursor.CurrentTile;

            // Show the tile cursor on the allowed tile area
            _TileCursor.MoveCursorTo(originTile);
            _TileCursor.Show();

            // Highlight the allowed tiles
            _TileIndicatorWidget.CreateIndicators(
                _AvailableTiles.Select(t => t.GlobalPosition),
                GetIndicatorColor()
            );

            // Request a diff (and include excess cost)
            RequestApDiff(new ActionPointHandler()
            {
                ActionPoints = _Action.ActionCost
            });

            // Connect to selection changed signal
            ChangedSelection += OnChangedSelection;
            // Initially select (in case we're targeting ourself)
            OnChangedSelection();
        }

        private IndicatorColor GetIndicatorColor()
        {
            switch (_Action.IndicatorType)
            {
                case Actions.ArenaAction.IndicatorTypes.Neutral:
                    {
                        return IndicatorColor.Blue;
                    }

                case Actions.ArenaAction.IndicatorTypes.Hostile:
                    {
                        return IndicatorColor.Red;
                    }

                case Actions.ArenaAction.IndicatorTypes.Friendly:
                    {
                        return IndicatorColor.Green;
                    }

                default:
                    {
                        return IndicatorColor.Blue;
                    }
            }
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // Confirm target and amount
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // Get selected tile from cursor
                var currentTile = _TileCursor.CurrentTile;

                // If we're not in the allowed moves, or the unit on the tile is not
                // ours, emit an invalid signal
                if (!_AvailableTiles.Contains(currentTile) || !_Action.ContainsValidTarget(currentTile))
                {

                    EmitSignal(nameof(ConfirmedInvalidSelection));
                    return;
                }

                // Get the target from the tile our cursor is on
                var target = currentTile.UnitOnTile;

                // Emit confirmation signal
                EmitSignal(nameof(ConfirmedSelection));

                // Get the positioning
                Positions position = Positions.Null;
                // If the target isn't us, and it's not a ranged attack
                if (_Action.Range > 1 && target != _SelectedUnit)
                {
                    position = Positioning.GetPositioning(_SelectedUnit.FacingDirection, target.FacingDirection);
                }

                // Run the action
                EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
                {
                    { Consts.ACTION_KEY, _Action },
                    { ActionConsts.TARGET, target },
                    { ActionConsts.POSITIONING, (int)position }
                });

                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Emit Cancellation Signal
                EmitSignal(nameof(CanceledSelection));

                // Persist if we moved at all during targeting
                var wasOnInitialTile = _TileCursor.CurrentTile == _InitialCursorTile;
                // Reset position and facing direction of the unit
                _TileCursor.MoveCursorTo(_InitialCursorTile);
                _SelectedUnit.FacingDirection = _InitialUnitFacingDir;
                // And if we were on the same square as when we started, cancel
                // back to the main menu
                if (wasOnInitialTile) EmitSignal(nameof(MainMenuRequested));

                return;
            }

            // Get transformed movement vector from input
            var moveVec = _DirectionalInputHelper.InputDir;

            // Ignore if no movement input
            if (moveVec == Vector3.Zero) return;

            // Get the candidate tile
            var candidateTile = _TileManager.GetTileAtTilemapPos(_TileCursor.CurrentTile.TilemapPosition + moveVec);
            var allTiles = _AvailableTiles.ToList().Append(_InitialCursorTile);
            if (candidateTile == null || !allTiles.Contains(candidateTile))
            {
                EmitSignal(nameof(ConfirmedInvalidSelection));
                return;
            }

            // Move the cursor in the direction of the movement
            _TileCursor.MoveCursorInDirection(moveVec);

            // If the cursor is on an available move, orient our unit towards
            // that square as best as possible
            if (_AvailableTiles.Contains(_TileCursor.CurrentTile))
            {
                var candidateDir = _SelectedUnit.GetFacingDirTowards(_TileCursor.CurrentTile.GlobalPosition);

                // Check for if we've selected ourselves and revert to initial direction
                if (candidateDir == Vector3.Zero) _SelectedUnit.FacingDirection = _InitialUnitFacingDir;
                // Otherwise just set the value we get
                else _SelectedUnit.FacingDirection = candidateDir;
            }

            // And Emit a signal that the selection changed
            EmitSignal(nameof(ChangedSelection));
        }

        public override void OnExit(string nextState)
        {
            base.OnExit(nextState);

            // Disconnect and clean up target UI
            ChangedSelection -= OnChangedSelection;
            _InfoUIContainer.CleanUp();

            // Reset state vars
            _InitialUnitFacingDir = Vector3.Zero;
            _AvailableTiles.Clear();

            // Hide and reset widgets
            _TileCursor.Hide();
            _TileCursor.MoveCursorTo(_InitialCursorTile);
            _TileIndicatorWidget.ClearIndicators();

            // Clear diff
            CancelApDiff();
        }

        private void OnChangedSelection()
        {
            // Clear and reset the current UI if we have one
            _InfoUIContainer.CleanUp();

            // Get selected tile from cursor
            var currentTile = _TileCursor.CurrentTile;

            // If the target on the current tile is a valid target and we have a
            // related UI for that action in the dictionary, then populate and
            // show that UI
            if (_Action.ContainsValidTarget(currentTile))
            {
                // Get the target from the tile our cursor is on
                var target = currentTile.UnitOnTile;

                // Handle the UI
                _InfoUIContainer.Init(target);
                _InfoUIContainer.SetDiff(1);
            }
        }
    }
}
