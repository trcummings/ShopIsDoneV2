using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using ShopIsDone.Arenas;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Widgets;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Actions;
using ShopIsDone.Utils;
using ShopIsDone.Tiles.UI;

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

        //private InteractableUIContainer _InteractableUIContainer;

        [Export]
        private TileHoverUI _TileHoverUI;

        [Export]
        private Control _EndPlayerTurnWidget;

        //private Control _MoreInfoUI;

        [Inject]
        private DirectionalInputHelper _InputHelper;

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private PlayerCameraService _PlayerCameraService;

        [Inject]
        private InputXformer _InputXformer;

        [Inject]
        private TileManager _TileManager;

        // Widgets
        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        //private TileIndicator _TileIndicatorWidget;

        // State
        private Tile _LastSelectedTile;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            // Get last selected tile from message to focus cursor on initially
            if (message?.ContainsKey(Consts.LAST_SELECTED_TILE_KEY) ?? false)
            {
                _LastSelectedTile = (Tile)message[Consts.LAST_SELECTED_TILE_KEY];
            }

            // Inject dependencies
            InjectionProvider.Inject(this);

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

            //    // Show the "More Info" UI
            //    _MoreInfoUI.Show();

            // Connect to tile cursor signal and the invalid move signal
            _TileCursor.CursorEnteredTile += OnCursorHoveredTile;
            _TileCursor.AttemptedUnavailableMove += OnAttemptedInvalidMove;

            // Show cursors
            _TileCursor.Show();
            _FingerCursor.Show();

            _PlayerCameraService.Activate();

            // Base start hook
            base.OnStart(message);
        }

        public override void UpdateState(double delta)
        {
            base.UpdateState(delta);

            //    // Camera and conditions input
            //    ProcessConditionsInput();
            //    ProcessCameraInput();

            //    // Check for "more info" input
            //    if (Input.IsActionJustPressed("open_more_info"))
            //    {
            //        // Do not open more info if tile is hidden
            //        if (!_LastSelectedTile.IsLit()) return;

            //        // SFX Feedback
            //        EmitSignal(nameof(SelectedUnit));

            //        // Grab the last selected entity
            //        var lastSelected = GetSelectableOnTile(_LastSelectedTile);
            //        // Change to "More Info" state with payload
            //        ChangeState("MoreInfoState", new Dictionary<string, Variant>()
            //        {
            //            { "Tile", _LastSelectedTile },
            //            { "Pawn", _LastSelectedTile.CurrentPawn },
            //            { "Interactable", lastSelected is Interactable ? lastSelected : null }
            //        });
            //        return;
            //    }

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
                    .Where(_PlayerUnitService.UnitHasAvailableActions)
                    .ToList()
                    .Find(e => e.TilemapPosition == _LastSelectedTile.TilemapPosition);

                // If null, emit an invalid selection signal
                if (unit == null)
                {
                    EmitSignal(nameof(AttemptedInvalidSelection));
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

            //    // Cycle through units input
            //    if (Input.IsActionJustPressed("cycle_player_pawns_forward"))
            //    {
            //        CycleActivePawns(1);
            //        return;
            //    }
            //    if (Input.IsActionJustPressed("cycle_player_pawns_backward"))
            //    {
            //        CycleActivePawns(-1);
            //        return;
            //    }

            //    // Cycle through tasks input
            //    if (Input.IsActionJustPressed("cycle_tasks_forward"))
            //    {
            //        CycleActiveTasks(1);
            //        return;
            //    }
            //    if (Input.IsActionJustPressed("cycle_tasks_backward"))
            //    {
            //        CycleActiveTasks(-1);
            //        return;
            //    }

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

            //    // Hide Interactable Container UI
            //    _InteractableUIContainer.Hide();

            // Hide tile UI
            _TileHoverUI.Hide();

            //    // Hide and clear indicator widget
            //    _TileIndicatorWidget.ClearIndicators();
            //    _TileIndicatorWidget.Hide();

            //    // Hide MoreInfo UI CTA
            //    _MoreInfoUI.Hide();

            //    // Unhighlight all interactables
            //    foreach (var hoverable in PTM.GetAllEntities().Where(e => e.HasComponent<HoverEntityComponent>()))
            //    {
            //        hoverable.GetComponent<HoverEntityComponent>()?.Unhover();
            //    }

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

            //    // If tile is not lit, we can't get more info about it, so hide/show
            //    var lightDetector = tile.GetComponent<LightDetectorComponent>();
            //    if (lightDetector.IsLit()) _MoreInfoUI.Show();
            //    else _MoreInfoUI.Hide();

            // Select Player Pawn UI if there's an active unit on the tile
            _PawnUIContainer.SelectPawnElement(GetActiveUnitOnTile(tile));

            //    // Handle interactable hover case
            //    SelectInteractable(tile);

            //    // Handle tile
            //    if (!_TileUIContainer.Visible) _TileUIContainer.Show();
            //    _TileUIContainer.SelectTile(tile);
        }

        private void OnAttemptedInvalidMove()
        {
            EmitSignal(nameof(AttemptedInvalidMove));
        }

        //private void SelectInteractable(Tile tile)
        //{
        //    // Unhighlight all interactables
        //    foreach (var hoverable in PTM.GetAllEntities().Where(e => e.HasComponent<HoverEntityComponent>()))
        //    {
        //        hoverable.GetComponent<HoverEntityComponent>()?.Unhover();
        //    }
        //    // Hide widgets and UI
        //    _InteractableUIContainer.Hide();
        //    _TileIndicatorWidget.Hide();
        //    _TileIndicatorWidget.ClearIndicators();

        //    // Ignore if selectable is null
        //    var selectable = GetSelectableOnTile(tile);
        //    if (selectable == null) return;

        //    // If not lit, ignore
        //    if (!selectable.GetComponent<LightDetectorComponent>().IsLit()) return;

        //    // If it doesn't have an interaction, ignore
        //    if (!selectable.HasComponent<InteractionComponent>()) return;

        //    var interactable = selectable;

        //    // Select Interactable Container UI if we're on an interactable
        //    _InteractableUIContainer.SelectInteractable(interactable);
        //    _InteractableUIContainer.Show();

        //    // Show indicators for selected tile if any
        //    if (interactable.IsActive())
        //    {
        //        // Show yellow tile indicators where the interactable has them
        //        var interaction = interactable.GetComponent<InteractionComponent>();
        //        _TileIndicatorWidget.CreateIndicators(
        //            interaction.GetInteractionTiles()
        //                .Where(iTile => iTile.Tile != null)
        //                .Select(iTile => iTile.Tile.Entity as Tile),
        //            TileIndicator.IndicatorColor.Yellow
        //        );

        //        // Highlight this interactable if it's available
        //        if (interactable.IsInArena()) interactable.GetComponent<HoverEntityComponent>()?.Hover();
        //    }
        //}

        //// Cycling Pawns and Tasks
        //private void CycleActivePawns(int dir)
        //{
        //    // Get all active units
        //    // NB: We can safely assume that there will be at least one to warp to
        //    var activeUnits = PTM.GetActivePlayerPawns().Select(e => e as Pawn);

        //    // Set up a var for the next selected unit
        //    Pawn newUnit;

        //    // If we're not on a tile with a unit on it
        //    if (!_LastSelectedTile.HasUnitOnTile())
        //    {
        //        // Get the closest unit to the selected tile
        //        newUnit = PawnHelper.GetClosestPawn(activeUnits, _LastSelectedTile);
        //    }
        //    // Otherwise, out of the list of active units, go either forward or backwards in
        //    // the list based on our currently selected unit
        //    else
        //    {
        //        var currentUnit = _LastSelectedTile.CurrentPawn;
        //        var selectedIdx = activeUnits.ToList().IndexOf(currentUnit);

        //        // Select circularly
        //        newUnit = activeUnits.SelectCircular(selectedIdx, dir);
        //    }

        //    // Warp the cursors to the new tile
        //    _TileCursor.MoveCursorTo(newUnit.CurrentTile);
        //    _FingerCursor.WarpCursorTo(_TileCursor.CurrentTile.GlobalPosition);
        //}

        //private void CycleActiveTasks(int dir)
        //{
        //    // Grab the last selected entity
        //    var lastSelected = GetSelectableOnTile(_LastSelectedTile);
        //    // Grab all the active tasks in the arena
        //    var activeTaskInteractables = PTM.GetActiveTaskInteractables();

        //    // If we don't have any active tasks, don't cycle,
        //    // but emit an error signal and return early
        //    if (activeTaskInteractables.Count() == 0)
        //    {
        //        EmitSignal(nameof(AttemptedInvalidSelection));
        //        return;
        //    }

        //    // Check if we're on a tile with an active task on it
        //    var selectedTileHasActiveTaskInteractable = activeTaskInteractables.Contains(lastSelected);

        //    // If we're not on a tile with an active task on it, then get the closest task's selectable tile
        //    if (!selectedTileHasActiveTaskInteractable)
        //    {
        //        var nextTile = PTM.GetAllTiles()
        //            // Winnow down to tiles that are in our active tasks
        //            .Where(tile => activeTaskInteractables.Contains(GetSelectableOnTile(tile)))
        //            // Get the closest tile based on the tilemap position
        //            .OrderBy(tile => tile.TilemapPosition.DistanceTo(_LastSelectedTile.TilemapPosition))
        //            // Grab the first item in the list
        //            // NB: There will always be an item in the list here because our previous case filtered
        //            // out situations where there's no active tasks
        //            .FirstOrDefault();

        //        // Warp to the next tile
        //        _TileCursor.MoveCursorTo(nextTile);
        //        _FingerCursor.WarpCursorTo(_TileCursor.CurrentTile.GlobalPosition);

        //        // Return early
        //        return;
        //    }

        //    // But, if we're on the only task, emit an error signal and return early
        //    else if (selectedTileHasActiveTaskInteractable && activeTaskInteractables.Count() == 1)
        //    {
        //        EmitSignal(nameof(AttemptedInvalidSelection));
        //        return;
        //    }

        //    // Otherwise, out of the list of active tasks, go forwards or backwards in
        //    // the list based on our current tile, and pick the first selection tile we get, so first,
        //    // Find our currently selected task
        //    // NB: We know for sure this exists because of the previous two cases
        //    var currentTaskInteractable = lastSelected;
        //    var selectedIdx = activeTaskInteractables.ToList().IndexOf(currentTaskInteractable);

        //    // Select circularly
        //    var newTaskInteractable = activeTaskInteractables.SelectCircular(selectedIdx, dir);

        //    // Pick the first tile that references that task interactable
        //    var newTile = PTM.GetAllTiles()
        //        .Where(tile => GetSelectableOnTile(tile) == newTaskInteractable)
        //        .FirstOrDefault();

        //    // Warp the cursors to the new tile
        //    _TileCursor.MoveCursorTo(newTile);
        //    _FingerCursor.WarpCursorTo(_TileCursor.CurrentTile.GlobalPosition);
        //}

        //private LevelEntity GetSelectableOnTile(Tile tile)
        //{
        //    var selectables = GetSelectablesOnTile(tile);
        //    if (selectables.Count() == 0) return null;
        //    // FIXME: There can be two
        //    return selectables.First();
        //}

        //private List<LevelEntity> GetSelectablesOnTile(Tile tile)
        //{
        //    return PTM.GetAllEntities()
        //        .Where(e => e.IsOnTile(tile))
        //        .Where(e => e.HasComponent<SelectableComponent>())
        //        .ToList();
        //}
    }
}
