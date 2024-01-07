using System;
using Godot;
using ShopIsDone.Levels;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Interactables.Interactions
{
    // This is a composed interaction that wraps around a cutscene
    public partial class CutsceneInteraction : Interaction
	{
        [Export]
        private Interaction _DecoratedInteraction;

        [Inject]
        private CutsceneService _CutsceneService;

        public override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Start the cutscene
            _CutsceneService.StartCutscene();

            // Connect to the decorated interaction's Finished signal
            _DecoratedInteraction.Connect(
                nameof(_DecoratedInteraction.Finished),
                Callable.From(OnDecoratedInteractionFinished),
                (uint)ConnectFlags.OneShot
            );

            // Execute the interaction
            _DecoratedInteraction.Execute();
        }

        private void OnDecoratedInteractionFinished()
        {
            // Finish the cutscene
            _CutsceneService.FinishCutscene();

            // Finish this interaction
            Finish();
        }
    }
}

