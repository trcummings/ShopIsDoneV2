using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Core;

namespace ShopIsDone.ClownRules
{
    public partial class ClownRulesService : Node, IService
    {
        [Export]
        private bool _IsActive = false;

        [Export]
        private LevelEntity _Judge;

        // Check if any action rules have been broken and increment the clown
        // puppet's punishment meter, if anything breaks the threshold, add
        // punishment to the queue
        public Command ProcessActionRules(ArenaAction action, Dictionary<string, Variant> message)
        {
            return new Command();
        }

        // Resets each turn
        public void ResetActionRules()
        {
            // Loop over each rule and call reset
        }
    }
}

