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

namespace ShopIsDone.Arenas.UnitPlacement
{
	public partial class SelectingUnitPlacementState : State
	{
		[Export]
		private InitializingArenaState _InitState;

        [Export]
        private TileManager _TileManager;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;

        // Widgets
        [Inject]
        private FingerCursor _FingerCursor;

        [Inject]
        private TileCursor _TileCursor;

        [Export]
        private HeldInputHelper _ConfirmPlacementInput;

        [Export]
        private ConfirmPlacementPrompt _ConfirmPrompt;

        [Export]
        private float _HoldConfirmTime = 1.5f;


        public override void _Ready()
		{

		}

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Inject dependencies
            InjectionProvider.Inject(this);

            // Get player units
            var allUnits = _PlayerCharacterManager.GetAllUnits();

            // Get placement tiles
            var placementTiles = _TileManager
                .GetPlacementTiles()
                .Take(allUnits.Count)
                .ToArray();

            // Place the player units on the first few placement tiles we have
            for (int i = 0; i < placementTiles.Length; i++)
            {
                var placement = placementTiles[i];
                var unit = allUnits[i];
                unit.GlobalPosition = placement.GlobalPosition;
                // Set the unit's facing direction to the placement tile's direction
                unit.FacingDirection = placement.PlacementFacingDir;
            }

            // Place cursor on leader
            var leader = _PlayerCharacterManager.Leader;
            var leaderTile = _TileManager.GetTileAtGlobalPos(leader.GlobalPosition);

            // Move the cursors to the last selected tile
            _TileCursor.MoveCursorTo(leaderTile);
            _FingerCursor.WarpCursorTo(leaderTile.GlobalPosition);

            // Show cursors
            _FingerCursor.Show();
            _TileCursor.Show();

            // Show confirm prompt
            _ConfirmPrompt.Show();
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
        }

        public override void OnExit(string nextState)
        {
            // Hide cursors
            _FingerCursor.Hide();
            _TileCursor.Hide();

            // Hide confirm prompt
            _ConfirmPrompt.Hide();

            base.OnExit(nextState);
        }
    }
}