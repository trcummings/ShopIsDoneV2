using Godot;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;
using ShopIsDone.Arenas.UI;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class InteractState : ActionState
    {
        [Export]
        private UnitInteractionService _UnitInteractionService;

        [Export]
        private InfoUIContainer _InfoUIContainer;

        private bool _JustConfirmedInteraction = false;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Reset flag
            _JustConfirmedInteraction = false;

            // Wire up unit interaction service
            _UnitInteractionService.ConfirmedInteraction += OnConfirmInteraction;
            _UnitInteractionService.CanceledInteraction += OnCancel;
            _UnitInteractionService.SelectedInteraction += OnSelectInteraction;
            _UnitInteractionService.Activate(_SelectedUnit);
        }

        public override void OnExit(string nextState)
        {
            // If next state is "Idle" then we probably got interrupted, reset
            // the service
            if (!_JustConfirmedInteraction && nextState == Consts.States.IDLE)
            {
                _UnitInteractionService.Reset();
            }

            // Reset flag
            _JustConfirmedInteraction = false;

            // Clean up unit interaction service
            _UnitInteractionService.ConfirmedInteraction -= OnConfirmInteraction;
            _UnitInteractionService.CanceledInteraction -= OnCancel;
            _UnitInteractionService.SelectedInteraction -= OnSelectInteraction;
            _UnitInteractionService.Deactivate();

            // Clean up ui container
            _InfoUIContainer.CleanUp();

            // Base OnExit
            base.OnExit(nextState);
        }

        private void OnCancel()
        {
            EmitSignal(nameof(MainMenuRequested));
        }

        private void OnConfirmInteraction(InteractionComponent interaction)
        {
            // Set flag
            _JustConfirmedInteraction = true;

            // Emit
            EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
            {
                { Consts.ACTION_KEY, _Action },
                { Consts.INTERACTION_KEY, interaction }
            });
        }

        private void OnSelectInteraction(InteractionComponent interaction)
        {
            // Handle the UI
            _InfoUIContainer.Init(interaction.Entity);
        }
    }
}