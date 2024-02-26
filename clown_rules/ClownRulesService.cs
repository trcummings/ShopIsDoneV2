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
using ShopIsDone.EntityStates;
using ShopIsDone.ActionPoints;
using ShopIsDone.Demerits;
using ShopIsDone.Microgames;
using ShopIsDone.Utils.Positioning;
using ShopIsDone.Microgames.Outcomes;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Cameras;
using ShopIsDone.Widgets;
using ApConsts = ShopIsDone.ActionPoints.Consts;
using ShopIsDone.Utils.Pathfinding;
using ShopIsDone.Tiles;
using SystemGenerics = System.Collections.Generic;
using Utils.Extensions;
using ShopIsDone.Lighting;

namespace ShopIsDone.ClownRules
{
    public partial class ClownRulesService : Node, IService, IInitializable, ICleanUpable
    {
        [Export]
        private bool _IsActive = false;

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

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private DemeritWidget _DemeritWidget;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ArenaLightService _ArenaLightService;

        [Export]
        private float _IndividualRageThreshold = 3f;

        [Export]
        private float _GroupRageThreshold = 1f;

        [Export]
        private LevelEntity _Judge;
        private EntityStateHandler _StateHandler;
        private MicrogameHandler _MicrogameHandler;

        [Export]
        private PackedScene _DebugPanelScene;
        private ClownDebug _DebugPanel;

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

        // Number of turns judge has been unhidden
        private const int _MinTurnsToStayOut = 3;
        private int _NumTurnsInArena = 0;

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
        }

        public void CleanUp()
        {
            // Remove clown debug panel
            var events = Events.GetEvents(this);
            events.EmitSignal(nameof(events.RemoveDebugPanelRequested), _DebugPanel);
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

            // Get judge's components
            _MicrogameHandler ??= _Judge.GetComponent<MicrogameHandler>();
            _StateHandler ??= _Judge.GetComponent<EntityStateHandler>();

            // Create and add debug panel
            _DebugPanel = _DebugPanelScene.Instantiate<ClownDebug>();
            var events = Events.GetEvents(this);
            events.EmitSignal(nameof(events.AddDebugPanelRequested), _DebugPanel);
            // Init debug panel
            _DebugPanel.Init(this);
        }

        // Check if any action rules have been broken and increment the clown
        // puppet's punishment meter, if anything breaks the threshold, add
        // punishment to the queue
        public Command ProcessActionRules(ArenaAction action, Dictionary<string, Variant> message)
        {
            return new ConditionalCommand(
                () => _IsActive && _PlayerUnitService.IsPlayerUnit(action.Entity),
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

                                    return new ActionCommand(() => {
                                        // Increment rage threshold
                                        IncreaseRage(action.Entity.Id);

                                        // Enqueue Punishment for after action is completed
                                        _ScriptQueueService.AddScriptToQueue(new CommandArenaScript()
                                        {
                                            CommandFn = ProcessPunishment
                                        });
                                    });
                                })
                                .ToArray()
                        ),
                        // Puppet positioning
                        new DeferredCommand(UpdateJudgePosition)
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
                new SeriesCommand(
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
                                IncreaseRage(unit.Id, 0.5f);
                            }
                        }
                    }),
                    new DeferredCommand(ProcessPunishment),
                    new ActionCommand(SubsideRage),
                    // Update on turn
                    JudgeTurnUpdate()
                )
            );
        }

        private bool IsUnitAboveRageThreshold(string id)
        {
            return _UnitRage[id] >= _IndividualRageThreshold;
        }

        private bool IsAboveGroupRageThreshold()
        {
            return _GroupRage >= _GroupRageThreshold;
        }

        private Command ProcessPunishment()
        {
            return new SeriesCommand(
                // Update judge behavior / warnings
                new DeferredCommand(UpdateJudgeBehavior),
                // Punish individual units in order of greatest flagrance
                new SeriesCommand(
                    _PlayerUnitService
                        .GetUnits()
                        .Where(unit => IsUnitAboveRageThreshold(unit.Id))
                        .OrderBy(unit => _UnitRage[unit.Id])
                        .Select(PunishUnit)
                        .ToArray()
                ),
                new DeferredCommand(() => new ConditionalCommand(
                    IsAboveGroupRageThreshold,
                    PunishGroup()
                )),
                // Update judge behavior / warnings again
                new DeferredCommand(UpdateJudgeBehavior)
            );
        }

        private Command UpdateJudgePosition()
        {
            // If a unit gets too close, warp away to a better position away from
            // the action
            return new ConditionalCommand(
                // If we're not hidden
                () => !_StateHandler.IsInState(StateConsts.ClownPuppet.HIDDEN) &&
                // If any unit is within 1 tile
                _PlayerUnitService
                    .GetUnits()
                    .Any(unit => unit.TilemapPosition.ManhattanDistance(_Judge.TilemapPosition) <= 1),
                // Then warp to an available tile
                _CameraService.PanToTemporaryCameraTarget(
                    _TileManager.GetTileAtTilemapPos(_Judge.TilemapPosition),
                    new SeriesCommand(
                        // Warp clown away
                        WarpJudgeOut(),
                        // Pick random tile to warp to and warp there
                        new DeferredCommand(() => WarpJudgeIn(
                            GetAvailableTilesForJudge()
                                .ToList()
                                .Shuffle()
                                .First()
                        )),
                        // Idle judge
                        _StateHandler.RunChangeState(StateConsts.IDLE)
                    )
                )
            );
        }

        private Command WarpJudgeOut()
        {
            return new SeriesCommand(
                new ParallelCommand(
                    new AsyncCommand(async () => await ToSignal(
                        GetTree()
                            .CreateTween()
                            .TweenProperty(_Judge, "global_position", _Judge.GlobalPosition + 2 * Vector3.Up, 0.25f),
                        "finished"
                    )),
                    _StateHandler.RunChangeState(StateConsts.ClownPuppet.WARP_OUT)
                ),
                _StateHandler.RunChangeState(StateConsts.ClownPuppet.HIDDEN),
                new ActionCommand(() =>
                {
                    // Move to far off point
                    _Judge.GlobalPosition = Vec3.FarOffPoint;
                })
            );
        }

        private Command WarpJudgeIn(Tile toTile)
        {
            return new SeriesCommand(
                // Warp to given tile
                new ActionCommand(() =>
                {
                    _Judge.GlobalPosition = toTile.GlobalPosition + 2 * Vector3.Up;
                }),
                // Warp back in
                new DeferredCommand(() => new ParallelCommand(
                    new AsyncCommand(async () => await ToSignal(
                        GetTree()
                            .CreateTween()
                            .TweenProperty(_Judge, "global_position", _Judge.GlobalPosition - 2 * Vector3.Up, 0.25f),
                        "finished"
                    )),
                    _StateHandler.RunChangeState(StateConsts.ClownPuppet.WARP_IN)
                ))
            );
        }

        private Array<Tile> GetAvailableTilesForJudge()
        {
            // First, get a set of all the spaces too close to existing
            // units and add them to a hash set
            var invalidTiles = _PlayerUnitService
                .GetUnits()
                .Aggregate(new SystemGenerics.HashSet<Tile>(), (acc, unit) =>
                {
                    var unitTile = _TileManager.GetTileAtTilemapPos(unit.TilemapPosition);
                    var tiles = unitTile?.GetTilesInRange(2) ?? new Array<Tile>();
                    foreach (var tile in tiles) acc.Add(tile);
                    return acc;
                });
            // Filter out all the unoccupied tiles by the invalid tile
            // set to get all the tiles we could potentially warp to
            var availableTiles = _TileManager
                .GetAllTilesInArena()
                .Where(tile =>
                    !tile.HasObstacleOnTile &&
                    !tile.HasUnitOnTile() &&
                    !invalidTiles.Contains(tile)
                )
                .ToGodotArray();

            // Take a poisson sample of the valid tiles to get a nicer
            // distribution
            var poisson = new TilePoissonDiskSampler(1, 3);
            return poisson.Generate(availableTiles);
        }

        private Command UpdateJudgeBehavior()
        {


            // If any units are above 50% rage, warn, and same with group percent

            // If we're above 25%, appear + message, below, disappear + message.
            // Make sure we stay out for at least a few turns

            return new SeriesCommand(
                //// HIGH RAGE
                // If we're above 75% group rage, change the lights, if we're below, make them
                // normal again
                new IfElseCommand(
                    () => _GroupRage >= _GroupRageThreshold * 0.75,
                    new ConditionalCommand(
                        () => !_ArenaLightService.AreLightsSet(),
                        new AsyncCommand(_ArenaLightService.SetAllLightsToColorAsync)
                    ),
                    new ConditionalCommand(
                        _ArenaLightService.AreLightsSet,
                        new AsyncCommand(_ArenaLightService.RevertAllLightColorsAsync)
                    )
                ),

                //// LOW RAGE
                // If we're above 25% on any type of rage, and hidden, warp in
                new ConditionalCommand(
                    () =>
                        (_GroupRage >= _GroupRageThreshold * 0.25 ||
                        _UnitRage.Values.Any(val => val >= _IndividualRageThreshold * 0.25f)) &&
                        _StateHandler.IsInState(StateConsts.ClownPuppet.HIDDEN),
                    // Appear
                    new SeriesCommand(
                        WarpJudgeIn(
                            GetAvailableTilesForJudge()
                                .ToList()
                                .Shuffle()
                                .First()
                        ),
                        // Idle judge
                        _StateHandler.RunChangeState(StateConsts.IDLE)
                    )
                ),
                // If we're below 25% on all rage, not hidden, and have been out
                // for at least 3 turns
                new ConditionalCommand(
                    () => (
                        !_StateHandler.IsInState(StateConsts.ClownPuppet.HIDDEN) &&
                        _NumTurnsInArena >= _MinTurnsToStayOut &&
                        _GroupRage <= _GroupRageThreshold * 0.25 &&
                        _UnitRage.Values.All(val => val <= _IndividualRageThreshold * 0.25f)
                    ),
                    // Warp out
                    new SeriesCommand(
                        WarpJudgeOut(),
                        // And reset the number of turns in the arena
                        new ActionCommand(() => _NumTurnsInArena = 0)
                    )
                )
            );
        }

        private Command JudgeTurnUpdate()
        {
            // If we're not hidden, tick up the number of turns not hidden
            return new ConditionalCommand(
                () => !_StateHandler.IsInState(StateConsts.ClownPuppet.HIDDEN),
                new ActionCommand(() => _NumTurnsInArena += 1)
            );
        }

        private Command PunishUnit(LevelEntity unit)
        {
            var apHandler = unit.GetComponent<ActionPointHandler>();
            var demeritHandler = unit.GetComponent<DemeritHandler>();
            var stateHandler = unit.GetComponent<EntityStateHandler>();

            return new SeriesCommand(
                // Face judge towards offender
                new ActionCommand(() => _Judge.FacingDirection = _Judge.GetFacingDirTowards(unit.GlobalPosition)),
                // Raise judge arm
                FocusTarget(
                    _Judge,
                    _StateHandler.RunChangeState(StateConsts.ClownPuppet.PUNISH)
                ),
                // Oneshot connect to slap for unit take hit
                new ActionCommand(() =>
                {
                    _DemeritWidget.Connect(
                        nameof(_DemeritWidget.DemeritSlapped),
                        Callable.From(() => stateHandler.PushState(StateConsts.HURT)),
                        (uint)ConnectFlags.OneShot
                    );
                }),

                // Apply demerit punishment
                _CameraService.PanToTemporaryCameraTarget(
                    unit,
                    _CameraService.TemporaryCameraZoom(
                    new IfElseCommand(
                        () => !demeritHandler.HasYellowSlip,
                        // Give them the yellow slip
                        new SeriesCommand(
                            demeritHandler.EscalateDemeritStatus(),
                            new AsyncCommand(() => _DemeritWidget.GrantDemeritAsync(
                                unit.GlobalPosition,
                                DemeritWidget.DemeritType.YellowSlip
                            ))
                        ),
                        // Otherwise, give them the pink slip and punish them severely
                        new SeriesCommand(
                            demeritHandler.EscalateDemeritStatus(),
                            new AsyncCommand(() => _DemeritWidget.GrantDemeritAsync(
                                unit.GlobalPosition,
                                DemeritWidget.DemeritType.PinkSlip
                            )),
                            apHandler.TakeAPDamage(new Dictionary<string, Variant>()
                            {
                                { ApConsts.DAMAGE_SOURCE, unit },
                                // Infinite damage
                                { ApConsts.DAMAGE_AMOUNT, 1000000000 }
                            })
                        )
                    ))
                ),
                // Lower arm
                _StateHandler.RunChangeState(StateConsts.ClownPuppet.FINISH_PUNISH),
                // Subside individual rage by threshold amount
                new ActionCommand(() =>
                {
                    _UnitRage[unit.Id] = Mathf.Max(_UnitRage[unit.Id] - _IndividualRageThreshold, 0);
                    // Tick down group rage by a little bit
                    _GroupRage = Mathf.Max(_GroupRage - 0.2f, 0);
                })
            );
        }

        private Command PunishGroup()
        {
            // Get all unit outcome handlers
            var outcomeHandlers = _PlayerUnitService
                .GetUnits()
                .Select(unit => unit.GetComponent<IOutcomeHandler>())
                .ToArray();

            var payload = new MicrogamePayload()
            {
                Targets = outcomeHandlers,
                Position = Positions.Null,
                Message = new Dictionary<string, Variant>()
            };

            return new SeriesCommand(
                FocusTarget(
                    _Judge,
                    _StateHandler.RunChangeState(StateConsts.ClownPuppet.PUNISH)
                ),
                // TODO: Kill the lights

                // Punish group with a microgame that impacts all of them
                _MicrogameHandler.RunMicrogame(payload),

                // TODO: Bring lights back on

                // Lower arm
                _StateHandler.RunChangeState(StateConsts.ClownPuppet.FINISH_PUNISH),

                // Subside all rage by threshold amount
                new ActionCommand(() =>
                {
                    foreach (var key in _UnitRage.Keys)
                    {
                        _UnitRage[key] = Mathf.Max(_UnitRage[key] - _IndividualRageThreshold, 0);
                    }
                    _GroupRage = Mathf.Max(_GroupRage - _GroupRageThreshold, 0);
                })
            );
        }

        private Command FocusTarget(LevelEntity target, Command next = null)
        {
            return _CameraService.PanToTemporaryCameraTarget(
                target,
                _CameraService.TemporaryCameraZoom(
                    _CameraService.RunRotateCameraTo(_Judge.FacingDirection,
                        next ?? new Command()
                    ),
                    0.25f
                )
            );
        }

        private void IncreaseRage(string id, float ratio = 1f)
        {
            // Flip rage flag
            _WasEnragedThisTurn = true;

            // Increment rage
            _UnitRage[id] += 1 * ratio;
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

