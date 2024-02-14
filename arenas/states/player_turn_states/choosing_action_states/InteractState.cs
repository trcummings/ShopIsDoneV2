using Godot;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class InteractState : ActionState
    {
        [Export]
        private UnitInteractionService _UnitInteractionService;

        private bool _JustConfirmedInteraction = false;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Reset flag
            _JustConfirmedInteraction = false;

            // Wire up unit interaction service
            _UnitInteractionService.ConfirmedInteraction += OnConfirmInteraction;
            _UnitInteractionService.CanceledInteraction += OnCancel;
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
            _UnitInteractionService.Deactivate();

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
    }
}