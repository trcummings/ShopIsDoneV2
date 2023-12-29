using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Arenas.PlayerTurn.ChoosingActions.Menu;
using ShopIsDone.ActionPoints;
using ShopIsDone.Core;
using System.Linq;
using PlayerTurnConsts = ShopIsDone.Arenas.PlayerTurn.Consts;

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
        private OptionsMenu _OptionsMenu;

        // State
        private bool IsFirstSelection = false;

        private LevelEntity _SelectedUnit;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Reset first selection var
            IsFirstSelection = true;

            // Pull the sub menu action out of the message
            var subMenuAction = (SubMenuAction)message["SubMenuAction"];
            // Get Selected unit
            _SelectedUnit = (LevelEntity)message[PlayerTurnConsts.SELECTED_UNIT_KEY];

            // Initialize the menu actions with the pawn
            foreach (var action in subMenuAction.MenuActions.OfType<ActionMenuAction>())
            {
                action.Pawn = _SelectedUnit;
            }

            // Connect to option menu signals
            _OptionsMenu.ConfirmedSelection += OnConfirmSelection;
            _OptionsMenu.CanceledSelection += OnCancelSelection;
            _OptionsMenu.ChangedSelection += OnChangeSelection;
            _OptionsMenu.SelectedDisabledOption += OnInvalidSelection;

            // Populate the menu with the list of moves
            _OptionsMenu.Activate(new OptionsMenu.Props()
            {
                InitialSelectedIndex = 0,
                MenuTitle = subMenuAction.Title,
                // Filter out the invisible actions
                MenuActions = subMenuAction.MenuActions.Where(action => action.IsVisible()).ToList()
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

        private void OnConfirmSelection(MenuAction menuAction)
        {
            // Select the action
            menuAction.SelectAction(this);

            // Emit signal
            EmitSignal(nameof(Confirmed));
        }

        private void OnChangeSelection(MenuAction menuAction)
        {
            // If this action has a cost, emit
            if (menuAction.IsAvailable() && menuAction is ActionMenuAction sAction)
            {
                var apHandler = _SelectedUnit.GetComponent<ActionPointHandler>();
                RequestApDiff(new ActionPointHandler()
                {
                    ActionPoints = sAction.Action.ActionCost
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

        protected void RequestApDiff(ActionPointHandler subtractableAp)
        {
            var apHandler = _SelectedUnit.GetComponent<ActionPointHandler>();
            EmitSignal(nameof(PawnAPDiffRequested), new ActionPointHandler()
            {
                ActionPoints = Mathf.Max(apHandler.ActionPoints - subtractableAp.ActionPoints, 0),
                ActionPointDebt = Mathf.Max(apHandler.ActionPointDebt - subtractableAp.ActionPointDebt, 0),
                ActionPointExcess = Mathf.Max(apHandler.ActionPointExcess - subtractableAp.ActionPointExcess, 0)
            });
        }

        protected void CancelApDiff()
        {
            EmitSignal(nameof(PawnAPDiffCanceled));
        }
    }
}