using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Cameras;
using ShopIsDone.Tiles;
using ShopIsDone.Widgets;
using System.Collections.Generic;
using GodotCollections = Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using PlayerTurnConsts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.ActionPoints;
using System.Linq;
using ShopIsDone.Utils;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
	public partial class MoveState : ActionState
    {
        [Signal]
        public delegate void ConfirmedPathEventHandler();

        [Signal]
        public delegate void CanceledPathEventHandler();

        [Signal]
        public delegate void AttemptedInvalidMoveEventHandler();

        [Signal]
        public delegate void ConfirmedInvalidPathEventHandler();

        [Signal]
        public delegate void UpdatedPathEventHandler();

        // Nodes
        [Export]
        private EffortMeter _EffortMeter;

        [Inject]
        private TileIndicator _TileIndicator;

        [Inject]
        private MovePathWidget _MovePathWidget;

        [Inject]
        private DirectionalInputHelper _InputHelper;

        [Inject]
        private ScreenshakeService _Screenshake;

        [Inject]
        private TileManager _TileManager;

        // Message vars
        private MoveAction _MoveAction;
        private LevelEntity _SelectedUnit;
        private Tile _CurrentTile;
        private ActionHandler _ActionHandler;
        private ActionPointHandler _ActionPointHandler;
        private TileMovementHandler _TileMoveHandler;

        // State vars
        private List<Tile> _MovePath = new List<Tile>();
        private List<Tile> _AllPossibleMoves = new List<Tile>();
        private List<Tile> _RemainingMoves = new List<Tile>();

        public override void OnStart(GodotCollections.Dictionary<string, Variant> message = null)
        {
            // Inject dependencies
            InjectionProvider.Inject(this);

            // Connect screenshake handler to events
            ConfirmedInvalidPath += OnScreenshake;
            AttemptedInvalidMove += OnScreenshake;

            // Connect to effort meter events
            _EffortMeter.InvalidSelection += OnInvalidEffort;
            _EffortMeter.Activated += OnEffortActivated;
            _EffortMeter.Deactivated += OnEffortDeactivated;
            _EffortMeter.Incremented += OnEffortIncremented;
            _EffortMeter.Decremented += OnEffortDecremented;

            // Grab the move action from the message
            _MoveAction = (MoveAction)message[Consts.ACTION_KEY];
            _SelectedUnit = (LevelEntity)message[PlayerTurnConsts.SELECTED_UNIT_KEY];
            _CurrentTile = _TileManager.GetTileAtTilemapPos(_SelectedUnit.TilemapPosition);
            _ActionHandler = _SelectedUnit.GetComponent<ActionHandler>();
            _ActionPointHandler = _SelectedUnit.GetComponent<ActionPointHandler>();
            _TileMoveHandler = _SelectedUnit.GetComponent<TileMovementHandler>();

            // Clear the move path and available moves
            _MovePath.Clear();
            // Add the current location to the move path
            _MovePath.Add(_CurrentTile);

            // Clear available moves and all possible moves
            _AllPossibleMoves.Clear();
            _RemainingMoves.Clear();

            // Get the action cost and the prior effort cost (maxed to the pawn's current AP)
            var actionCost = _MoveAction.ActionCost;
            var effortCost = _MoveAction.GetEffortSpent();

            // Init the effort meter with the effort and the pawn's available AP
            _EffortMeter.Init(effortCost, _ActionPointHandler.ActionPoints - actionCost);
            _EffortMeter.Show();

            // Get all possible moves for the selected pawn
            var moves = _TileMoveHandler.GetAvailableMoves(null, true, GetAdjustedMoveRange());
            _AllPossibleMoves = moves.ToList();
            _RemainingMoves = moves.ToList();

            // Show Tile Indicator Widget
            _TileIndicator.Show();

            // Show indicators on the tiles that the pawn can move on
            foreach (var move in _AllPossibleMoves)
            {
                _TileIndicator.CreateIndicator(move.GlobalPosition, TileIndicator.IndicatorColor.Blue);
            }

            // Initialize the the path indicator with the current location and show it
            _MovePathWidget.SetPath(GetMovePathPositions());
            _MovePathWidget.Show();

            // Show the movement cost
            RequestApDiff(_ActionPointHandler, new ActionPointHandler()
            {
                ActionPoints = actionCost + effortCost
            });

            // Connect to path change event
            UpdatedPath += OnPathChanged;

            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            // If we're using the meter, do not process other input
            if (Input.IsActionPressed("effort_meter"))
            {
                if (Input.IsActionJustPressed("effort_meter"))
                {
                    // Activate the meter
                    _EffortMeter.Activate();
                }
                else
                {
                    // Test increment / decrement input
                    if (Input.IsActionJustPressed("move_up") || Input.IsActionJustPressed("move_right"))
                    {
                        _EffortMeter.Increment();
                    }
                    else if (Input.IsActionJustPressed("move_down") || Input.IsActionJustPressed("move_left"))
                    {
                        _EffortMeter.Decrement();
                    }
                }

                return;
            }
            // If we just released though, we can process on the same frame
            else if (Input.IsActionJustReleased("effort_meter"))
            {
                // Deactivate the meter
                _EffortMeter.Deactivate();
            }

            // Confirm movement path
            if (Input.IsActionJustPressed("ui_accept"))
            {
                // If it's an invalid move path, emit invalid
                if (!_TileMoveHandler.IsValidMovePath(_MovePath))
                {
                    EmitSignal(nameof(ConfirmedInvalidPath));
                    return;
                }

                // Otherwise, emit a confirmation signal
                EmitSignal(nameof(ConfirmedPath));

                // Request run the move action with the move path
                var moveArray = new GodotCollections.Array<Tile>();
                foreach (var tile in _MovePath) moveArray.Add(tile);
                EmitSignal(nameof(RunActionRequested), new GodotCollections.Dictionary<string, Variant>()
                {
                    { Consts.ACTION_KEY, _MoveAction },
                    { MoveAction.MOVE_PATH_KEY, moveArray }
                });
                return;
            }

            // Revert / Go back
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                // If we have more than just one tile in the move set, revert to
                // the first tile
                if (_MovePath.Count > 1)
                {
                    // Emit signal
                    EmitSignal(nameof(CanceledPath));

                    // Clear the path
                    _MovePath.Clear();

                    // Add back in the selected unit's initial position
                    _MovePath.Add(_CurrentTile);

                    // Manually update the path
                    OnPathChanged();
                    return;
                }

                // Otherwise, cancel the move action and go back a menu level
                EmitSignal(nameof(CanceledPath));
                EmitSignal(nameof(MainMenuRequested));
                return;
            }

            // Ignore if no movement input
            if (_InputHelper.InputDir == Vector3.Zero) return;

            // Get the tile we're interested in from the neighbors of the latest tile
            // on the stack
            var candidateTile = _MovePath.Last().GetNeighborAtDir(_InputHelper.InputDir);

            // If no tile, invalid move
            if (candidateTile == null)
            {
                EmitSignal(nameof(AttemptedInvalidMove));
                return;
            }

            // Calculate if the tile is already in the path
            var tileAlreadyInPath = _MovePath.Contains(candidateTile);
            // Calculate if the intended move is backwards through our path
            var isBackwardsMove =
                // As long as we have 2 items in the move list we can go back
                _MovePath.Count > 1 &&
                // If it's the second to latest tile, we're moving backwards
                _MovePath[_MovePath.Count - 2] == candidateTile;

            // If move is not in the list of remaining moves, emit an invalid event
            if (!_RemainingMoves.Contains(candidateTile))
            {
                EmitSignal(nameof(AttemptedInvalidMove));
                return;
            }

            // If our move would make us cross a previous tile that isn't the
            // previous move, it's an invalid path (no crossovers)
            if (tileAlreadyInPath && !isBackwardsMove)
            {
                EmitSignal(nameof(AttemptedInvalidMove));
                return;
            }

            // If the candidate is the prior move, but we're not crossing an
            // existing move, then we must be going backwards
            if (isBackwardsMove)
            {
                // Remove the latest tile from the move path instead of adding it
                _MovePath.RemoveAt(_MovePath.Count - 1);
            }
            // Otherwise, it's a valid addition, so add the move to the list
            else _MovePath.Add(candidateTile);

            // Emit an update event
            EmitSignal(nameof(UpdatedPath));
        }

        public override void OnExit(string nextState)
        {
            // Disconnect screenshake handler from events
            ConfirmedInvalidPath -= OnScreenshake;
            AttemptedInvalidMove -= OnScreenshake;

            // Disconnect from effort meter events
            _EffortMeter.InvalidSelection -= OnInvalidEffort;
            _EffortMeter.Activated -= OnEffortActivated;
            _EffortMeter.Deactivated -= OnEffortDeactivated;
            _EffortMeter.Incremented -= OnEffortIncremented;
            _EffortMeter.Decremented -= OnEffortDecremented;

            // Hide the tile path indicator
            _MovePathWidget.Hide();

            // Clear the move path and available moves
            _MovePath.Clear();
            _AllPossibleMoves.Clear();
            _RemainingMoves.Clear();

            // Clear the tile indicator and hide it
            _TileIndicator.ClearIndicators();
            _TileIndicator.Hide();

            // Clear the ap diff
            CancelApDiff();

            // Disconnect from path change event
            UpdatedPath -= OnPathChanged;

            // Hide the effort meter
            _EffortMeter.Hide();

            base.OnExit(nextState);
        }

        private int GetAdjustedMoveRange()
        {
            var additionalMove = _EffortMeter.CurrentIndex * _TileMoveHandler.MoveEffortMod;
            return Mathf.FloorToInt(_TileMoveHandler.BaseMove + additionalMove);
        }

        private List<Vector3> GetMovePathPositions()
        {
            return _MovePath.Select((Tile tile) => tile.GlobalPosition).ToList();
        }

        private void OnPathChanged()
        {
            // Set the new effort in the action
            _ActionHandler.SetActionEffort(_MoveAction.Id, _EffortMeter.CurrentIndex);

            // Compute the new all possible moves
            var adjustedMove = GetAdjustedMoveRange();
            _AllPossibleMoves = _TileMoveHandler.GetAvailableMoves(null, true, adjustedMove);

            // Compute the remaining possible moves
            // NB: The initial tile is always in the range, so adjust the count by 1
            var usedMoves = Mathf.Max(_MovePath.Count - 1, 0);
            _RemainingMoves = _TileMoveHandler.GetAvailableMoves(_MovePath.Last(), true, adjustedMove - usedMoves);
            // If there is a backwards move possible in the list, add it to the remaining moves
            if (_MovePath.Count > 1) _RemainingMoves.Add(_MovePath[_MovePath.Count - 2]);

            // Update the indicators
            _TileIndicator.ClearIndicators();
            var foreclosedMoves = _AllPossibleMoves.Except(_RemainingMoves);
            foreach (var move in _RemainingMoves)
            {
                _TileIndicator.CreateIndicator(move.GlobalPosition, TileIndicator.IndicatorColor.Blue);
            }
            foreach (var move in foreclosedMoves)
            {
                _TileIndicator.CreateIndicator(move.GlobalPosition, TileIndicator.IndicatorColor.Grey);
            }

            // Update the line
            _MovePathWidget.SetPath(GetMovePathPositions());
            // Update it's validity
            _MovePathWidget.SetIsValidPath(_TileMoveHandler.IsValidMovePath(_MovePath));
        }


        private void OnScreenshake()
        {
            // A little bit of screenshake just on the x-axis
            _Screenshake.Shake(
                ScreenshakeHandler.ShakePayload.ShakeSizes.Mild,
                ScreenshakeHandler.ShakeAxis.XOnly
            );
        }

        private void OnEffortIncremented()
        {
            UpdateEffort();
            EmitSignal(nameof(UpdatedPath));
        }

        private void OnEffortDecremented()
        {
            UpdateEffort();

            // If move path is longer than the available moves with the new level of
            // effort, truncate it by the amount allowed (+1 for initial tile)
            _MovePath = _MovePath.Take(GetAdjustedMoveRange() + 1).ToList();

            EmitSignal(nameof(UpdatedPath));
        }

        private void UpdateEffort()
        {
            // Set the new effort in the action
            _ActionHandler.SetActionEffort(_MoveAction.Id, _EffortMeter.CurrentIndex);

            // Update the cost diff
            var actionCost = _MoveAction.ActionCost;
            var effortCost = _MoveAction.GetEffortSpent();

            RequestApDiff(_ActionPointHandler, new ActionPointHandler()
            {
                ActionPoints = actionCost + effortCost
            });
        }

        private void OnInvalidEffort()
        {
            EmitSignal(nameof(AttemptedInvalidMove));
        }

        private void OnEffortActivated()
        {
            EmitSignal(nameof(ConfirmedPath));
        }

        private void OnEffortDeactivated()
        {
            EmitSignal(nameof(CanceledPath));
        }
    }
}