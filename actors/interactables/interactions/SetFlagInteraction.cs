using Godot;
using ShopIsDone.Levels;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
    public partial class SetFlagInteraction : Interaction
    {
        [Export]
        private string _FlagName;

        [Export]
        private bool _SetFlagTo = true;

        [Inject]
        private LevelData _LevelData;

        public override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);
            // Set flag
            _LevelData.SetFlag(_FlagName, _SetFlagTo);
            // Finish
            Finish();
        }
    }
}
