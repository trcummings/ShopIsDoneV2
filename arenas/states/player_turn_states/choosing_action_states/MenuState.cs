using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core;
using PlayerTurnConsts = ShopIsDone.Arenas.PlayerTurn.Consts;
using ShopIsDone.Actions;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class MenuState : ActionState
    {
        [Signal]
        public delegate void SelectedEventHandler();

        [Signal]
        public delegate void ConfirmedEventHandler();

        [Signal]
        public delegate void InvalidEventHandler();

        [Signal]
        public delegate void CanceledEventHandler();

        // Nodes
        [Export]
        public PlayerUnitService _PlayerUnitService;

        [Export]
        private OptionsMenu _OptionsMenu;

        // State
        private bool IsFirstSelection = false;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Reset first selection var
            IsFirstSelection = true;

            // Connect to option menu signals
            _OptionsMenu.ConfirmedSelection += OnConfirmSelection;
            _OptionsMenu.CanceledSelection += OnCancelSelection;
            _OptionsMenu.ChangedSelection += OnChangeSelection;
            _OptionsMenu.SelectedDisabledOption += OnInvalidSelection;

            // Populate the menu with the list of moves
            _OptionsMenu.Activate(new OptionsMenu.Props()
            {
                InitialSelectedIndex = 0,
                MenuTitle = "Turn",
                // Filter out the invisible actions
                Actions = _PlayerUnitService.GetVisibleActions(_SelectedUnit)
            });

            // Show the menu
            _OptionsMenu.Show();
        }

        public override void OnExit(string nextState)
        {
            // Hide the options menu
            _OptionsMenu.Hide();

            // Deactivate the options menu
            _OptionsMenu.Deactivate();

            // Disconnect from the options menu
            _OptionsMenu.ConfirmedSelection -= OnConfirmSelection;
            _OptionsMenu.CanceledSelection -= OnCancelSelection;
            _OptionsMenu.ChangedSelection -= OnChangeSelection;
            _OptionsMenu.SelectedDisabledOption -= OnInvalidSelection;

            // Request to clear any AP diffs
            CancelApDiff();

            base.OnExit(nextState);
        }

        private void OnInvalidSelection()
        {
            // Emit signal
            EmitSignal(nameof(Invalid));
        }

        private void OnCancelSelection()
        {
            // Emit signal
            EmitSignal(nameof(Canceled));

            // Go back
            EmitSignal(nameof(GoBackRequested));
        }

        private void OnConfirmSelection(ArenaAction action)
        {
            // Create message
            var message = new Dictionary<string, Variant>()
            {
                { Consts.ACTION_KEY, action },
                { PlayerTurnConsts.SELECTED_UNIT_KEY, _SelectedUnit }
            };

            // Change state based on action type
            switch (action.ActionType)
            {
                case ArenaAction.ActionTypes.Null:
                    {
                        EmitSignal(nameof(RunActionRequested), message);
                        break;
                    }

                case ArenaAction.ActionTypes.Move:
                    {
                        ChangeState(Consts.States.MOVE, message);
                        break;
                    }

                case ArenaAction.ActionTypes.Facing:
                    {
                        ChangeState(Consts.States.FACING_DIRECTION, message);
                        break;
                    }

                case ArenaAction.ActionTypes.Target:
                    {
                        ChangeState(Consts.States.TARGETING, message);
                        break;
                    }

                case ArenaAction.ActionTypes.Interact:
                    {
                        ChangeState(Consts.States.INTERACT, message);
                        break;
                    }

                case ArenaAction.ActionTypes.Task:
                    {
                        ChangeState(Consts.States.TASK, message);
                        break;
                    }
            }

            // Emit signal
            EmitSignal(nameof(Confirmed));
        }

        private void OnChangeSelection(ArenaAction action)
        {
            // If this action has a cost, emit
            if (action.IsAvailable())
            {
                RequestApDiff(new ActionPointHandler()
                {
                    ActionPoints = action.ActionCost
                });
            }
            // Otherwise, cancel the diff
            else
            {
                CancelApDiff();
            }

            // Emit signal if it's not our first time selecting an option
            if (IsFirstSelection) IsFirstSelection = false;
            else EmitSignal(nameof(Selected));
        }
    }
}