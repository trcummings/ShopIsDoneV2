using System;
using Godot;
using ShopIsDone.Levels;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Interactables.Interactions
{
    // This is an interaction just for finishing out a cutscene that we don't
    // auto finish in the initial CutsceneInteraction for one reason or another
    [Tool]
    public partial class FinishCutsceneInteraction : Interaction
	{
        [Inject]
        private CutsceneService _CutsceneService;

        public override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Finish the cutscene
            _CutsceneService.FinishCutscene();

            // Finish the interaction
            Finish();
        }
    }
}

