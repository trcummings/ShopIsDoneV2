using Godot;
using Godot.Collections;
using ShopIsDone.ArenaInteractions;

namespace ShopIsDone.Arenas.PlayerTurn.ChoosingActions
{
    public partial class InteractState : ActionState
    {
        [Export]
        private UnitInteractionService _UnitInteractionService;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            base.OnStart(message);

            // Wire up unit interaction service
            _UnitInteractionService.ConfirmedInteraction += OnConfirmInteraction;
            _UnitInteractionService.CanceledInteraction += OnCancel;
            _UnitInteractionService.Activate(_SelectedUnit);
        }

        public override void OnExit(string nextState)
        {
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
            EmitSignal(nameof(RunActionRequested), new Dictionary<string, Variant>()
            {
                { Consts.ACTION_KEY, _Action },
                { Consts.INTERACTION_KEY, interaction }
            });
        }
    }
}