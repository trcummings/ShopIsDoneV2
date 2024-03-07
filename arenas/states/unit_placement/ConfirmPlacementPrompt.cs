using Godot;
using System;

namespace ShopIsDone.Arenas.UnitPlacement.UI
{
    public partial class ConfirmPlacementPrompt : PanelContainer
    {
        [Export]
        private Control _RadialProgress;

        public void SetRadialProgress(float progress)
        {
            _RadialProgress.Set("progress", progress);
        }
    }
}

