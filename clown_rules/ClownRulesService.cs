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
using static ShopIsDone.Actions.ActionService;
using ShopIsDone.Employees.Actions;
using ShopIsDone.Entities.Employees.Actions;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Tasks;

namespace ShopIsDone.ClownRules
{
    public partial class ClownRulesService : Node, IService, IInitializable, ICleanUpable
    {
        [Signal]
        public delegate void InitializedEventHandler();

        [Signal]
        public delegate void CleanedUpEventHandler();

        [Export]
        private bool _IsActive = false;

        //[Export]
        //private LevelEntity _Judge;

        [Export]
        private RulesUI _RulesUI;

        [Export]
        private RulesList _RulesList;

        [Export]
        private ActionService _ActionService;

        [Export]
        private ScriptQueueService _ScriptQueueService;

        [Inject]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private float _IndividualRageThreshold;

        [Export]
        private float _GroupRageThreshold;

        // State
        private Array<ClownActionRule> _Rules = new Array<ClownActionRule>();
        public float GroupRage { get { return _GroupRage; } }
        private float _GroupRage = 0;

        public Dictionary<string, float> UnitRage { get { return _UnitRage; } }
        private Dictionary<string, float> _UnitRage = new Dictionary<string, float>();

        // Track broken rules per action
        private bool _WasEnragedThisTurn = false;
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

            // Oneshot connect
            _PlayerUnitService.Connect(
                nameof(_PlayerUnitService.Initialized),
                Callable.From(OnInitUnits),
                (uint)ConnectFlags.OneShot
            );

            EmitSignal(nameof(Initialized));
        }

        public void CleanUp()
        {
            EmitSignal(nameof(CleanedUp));
        }

        private void OnInitUnits()
        {
            // Init rage
            _GroupRage = 0;
            _UnitRage = _PlayerUnitService.GetUnits().Aggregate(new Dictionary<string, float>(), (acc, unit) =>
            {
                acc[unit.Id] = 0;
                return acc;
            });
            _WasEnragedThisTurn = false;
        }

        // Check if any action rules have been broken and increment the clown
        // puppet's punishment meter, if anything breaks the threshold, add
        // punishment to the queue
        public Command ProcessActionRules(ArenaAction action, Dictionary<string, Variant> message)
        {
            return new ConditionalCommand(
                () => _IsActive && _PlayerUnitService.GetUnits().Contains(action.Entity),
                new DeferredCommand(() =>
                    new SeriesCommand(
                        // Rule breaking
                        new SeriesCommand(
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
                                    return new ActionCommand(() => IncreaseRage(action.Entity.Id));
                                })
                                .ToArray()
                        )
                        // TODO: Punishment
                    )
                )
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

        public Command ProcessTurnRules()
        {
            return new ConditionalCommand(
                () => _IsActive,
                new ActionCommand(() =>
                {
                    // Apply each rule to each unit
                    foreach (var unit in _PlayerUnitService.GetUnits())
                    {
                        // Get that unit's actions from the action history
                        var actions = _ActionService
                            .ActionHistory
                            .Where(item => item.Action.Entity == unit)
                            .ToGodotArray();

                        if (!(
                            CheckIfMoved(unit, actions) ||
                            CheckIfDoingTask(unit, actions) ||
                            CheckIfHelpedCustomer(unit, actions))
                        )
                        {
                            IncreaseRage(unit.Id);
                        }
                    }

                    // Subside rage
                    SubsideRage();
                })
            );
        }

        private void IncreaseRage(string id)
        {
            // Flip rage flag
            _WasEnragedThisTurn = true;

            // Increment rage
            _UnitRage[id] += 1;
            _GroupRage += 0.2f;
        }

        private void SubsideRage()
        {
            // If we weren't enraged this turn, decrement all rage
            if (!_WasEnragedThisTurn)
            {
                foreach (var key in _UnitRage.Keys)
                {
                    _UnitRage[key] = Mathf.Max(_UnitRage[key] - 0.5f, 0);
                }
                _GroupRage = Mathf.Max(_GroupRage - 0.2f, 0);
            }

            // Reset rage flag
            _WasEnragedThisTurn = false;
        }

        //
        private Command ProcessPunishment()
        {
            //

            return new Command();
        }

        private bool CheckIfMoved(LevelEntity _, Array<ActionHistoryItem> history)
        {
            return history.Select(item => item.Action).OfType<MoveSubAction>().Count() > 1;
        }

        private bool CheckIfDoingTask(LevelEntity unit, Array<ActionHistoryItem> history)
        {
            return history
                .Select(item => item.Action)
                .Any(action => action is StartTaskAction) ||
                (unit?.GetComponent<TaskHandler>().HasCurrentTask() ?? false);
        }

        private bool CheckIfHelpedCustomer(LevelEntity _, Array<ActionHistoryItem> history)
        {
            return history.Select(item => item.Action).Any(action => action is HelpCustomerAction);
        }
    }
}

