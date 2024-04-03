using System;
using System.Collections.Generic;
using Godot;
using Utils.Extensions;

namespace ShopIsDone.Microgames.SaladBar.States
{
    public partial class PersonAtSaladBarState : AtSaladBarState
    {
        [Export]
        public int Hunger = 1;

        // Number of hands that spawn with the tongs
        [Export]
        public int NumHands = 1;

        protected override bool ShouldLeave()
        {
            return Hunger == 0;
        }

        protected override void GatherActions()
        {
            var actions = new List<Action>()
            {
                () => { Events.EmitSignal(nameof(Events.TongsRequested)); }
            };
            for (int i = 0; i < NumHands; i++)
            {
                actions.Add(() => { Events.EmitSignal(nameof(Events.NastyHandRequested)); });
            }
            _Actions = new Queue<Action>(actions.Shuffle());
        }

        protected override void OnGrabbedFood(Grabber _)
        {
            // Tick down hunger
            Hunger -= 1;
            Hunger = Mathf.Max(0, Hunger);
        }
    }
}

