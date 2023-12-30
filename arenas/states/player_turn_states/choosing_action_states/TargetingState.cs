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

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
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
        [Inject]
        private DirectionalInputHelper _DirectionalInputHelper;

        [Inject]
        private TileIndicator _TileIndicatorWidget;

        [Inject]
        private TileCursor _TileCursor;

        [Inject]
        private TileManager _TileManager;

        // State Variables
        private Vector3 _InitialUnitFacingDir = Vector3.Zero;
        private Tile _InitialCursorTile;
        private GenericCollections.List<Tile> _AvailableTiles = new GenericCollections.List<Tile>();

        // Message vars
        private int _TransferAmount = 0;

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

            // Grab transfer amount from payload
            _TransferAmount = (int)message?.GetValueOrDefault("TransferAmount", 0);

            // Choose the origin for the tile selection
            var originTile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);

            // Get the allowed tiles based on the range
            _AvailableTiles = new MoveGenerator()
                .GetAvailableMoves(originTile, canTargetSelf, range)
                // Filter tiles out with obstacles on them
                .Where(tile => !tile.HasObstacleOnTile)
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
            _TileIndicatorWidget.Show();

            // Request a diff (and include excess cost)
            RequestApDiff(new ActionPointHandler()
            {
                ActionPoints = _Action.ActionCost,
                ActionPointExcess = _TransferAmount
            });
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

                // Run the action
                EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
                {
                    { Consts.ACTION_KEY, _Action },
                    { "Target", target },
                    { "TransferAmount", _TransferAmount }
                });

                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // Emit Cancellation Signal
                EmitSignal(nameof(CanceledSelection));

                // If our cursor's current tile isn't the same as when it started,
                // revert
                if (_TileCursor.CurrentTile != _InitialCursorTile)
                {
                    _TileCursor.MoveCursorTo(_InitialCursorTile);
                    _SelectedUnit.FacingDirection = _InitialUnitFacingDir;
                }
                // Otherwise, if our chosen direction is the same as the initial
                // direction, request to cancel the current state instead
                else EmitSignal(nameof(MainMenuRequested));

                return;
            }

            // Get transformed movement vector from input
            var moveVec = _DirectionalInputHelper.InputDir;

            // Ignore if no movement input
            if (moveVec == Vector3.Zero) return;

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

            // Reset state vars
            _InitialUnitFacingDir = Vector3.Zero;
            _AvailableTiles.Clear();

            // Hide and reset widgets
            _TileCursor.Hide();
            _TileIndicatorWidget.Hide();
            _TileIndicatorWidget.ClearIndicators();

            // Clear diff
            CancelApDiff();
        }
    }
}
