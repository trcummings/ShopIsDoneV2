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
using ShopIsDone.Arenas;

namespace ShopIsDone.ClownRules
{
    public partial class ClownRulesService : Node, IService, IInitializable
    {
        [Export]
        private bool _IsActive = false;

        //[Export]
        //private LevelEntity _Judge;

        [Export]
        private RulesUI _RulesUI;

        [Export]
        private RulesList _RulesList;

        private Array<ClownActionRule> _Rules = new Array<ClownActionRule>();

        [Inject]
        private PlayerUnitService _PlayerUnitService;

        // State
        public float GroupRage { get { return _GroupRage; } }
        private float _GroupRage = 0;

        public Dictionary<string, float> UnitRage { get { return _UnitRage; } }
        private Dictionary<string, float> _UnitRage = new Dictionary<string, float>();

        // Track broken rules per action
        public Dictionary<ClownActionRule, bool> BrokenRules { get { return _BrokenRules; } }
        private Dictionary<ClownActionRule, bool> _BrokenRules = new Dictionary<ClownActionRule, bool>();

        public void Init()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Get all rule nodes
            this.RecurseChildrenOfType<ClownActionRule>((_, rule) => InjectionProvider.Inject(rule));
            _Rules = GetChildren().OfType<ClownActionRule>().ToGodotArray();

            // Init ui
            _RulesUI.Init(_Rules);
            _RulesList.Init(_Rules);

            // Init rage
            _GroupRage = 0;
            _UnitRage = _PlayerUnitService.GetUnits().Aggregate(new Dictionary<string, float>(), (acc, unit) =>
            {
                acc[unit.Id] = 0;
                return acc;
            });
        }

        // Check if any action rules have been broken and increment the clown
        // puppet's punishment meter, if anything breaks the threshold, add
        // punishment to the queue
        public Command ProcessActionRules(ArenaAction action, Dictionary<string, Variant> message)
        {
            return new ConditionalCommand(
                () => _IsActive,
                new DeferredCommand(() => new SeriesCommand(
                    _Rules
                        .Where(rule =>
                            // if we haven't broken this rule yet
                            !_BrokenRules.ContainsKey(rule) &&
                            // And it's broken
                            rule.BrokeRule(action, message)
                        )
                        .Select(rule =>
                        {
                            // Add to broken rules
                            _BrokenRules.Add(rule, true);

                            // Increment rage threshold
                            return new ActionCommand(() => {

                            });
                        })
                        .ToArray()
                ))
            );
        }

        // Resets after each full action (not called after sub actions are
        // finished)
        public void ResetActionRules()
        {
            if (!_IsActive) return;

            // Loop over each rule and call reset
            foreach (var rule in _Rules) rule.ResetRule();

            // Clear broken rules
            _BrokenRules.Clear();
        }
    }
}

