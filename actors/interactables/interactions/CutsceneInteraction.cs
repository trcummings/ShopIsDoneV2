using System;
using Godot;
using ShopIsDone.Levels;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Interactables.Interactions
{
    // This is a composed interaction that wraps around a cutscene
    [Tool]
    public partial class CutsceneInteraction : DecoratorInteraction
    {
        // When we start a cutscene, we go to the level's Cutscene state, but we
        // may not always want to go directly back to what we were doing before.
        // Disabling this allows you to manually go to a different state later,
        // e.g. if you want to change levels
        [Export]
        private bool _RevertCutsceneStateOnFinish = true;

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
            if (_RevertCutsceneStateOnFinish) _CutsceneService.FinishCutscene();

            // Finish this interaction
            // NB: Deferred just in case there was any input during the last
            // frame
            CallDeferred(nameof(Finish));
        }
    }
}

