using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Core;
using ShopIsDone.Utils;
using ShopIsDone.ClownRules.UI;
using ShopIsDone.ClownRules.ActionRules;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.ClownRules
{
    public partial class ClownRulesService : Node, IService, IInitializable
    {
        [Export]
        private bool _IsActive = false;

        [Export]
        private LevelEntity _Judge;

        [Export]
        private RulesUI _RulesUI;

        [Export]
        private RulesList _RulesList;

        private Array<ClownActionRule> _Rules = new Array<ClownActionRule>();

        public void Init()
        {
            this.RecurseChildrenOfType<ClownActionRule>((_, rule) => InjectionProvider.Inject(rule));
            _Rules = GetChildren().OfType<ClownActionRule>().ToGodotArray();

            // Init ui
            _RulesUI.Init(_Rules);
            _RulesList.Init(_Rules);
        }

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

