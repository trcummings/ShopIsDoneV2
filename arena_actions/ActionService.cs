using Godot;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Cameras;
using ShopIsDone.Tiles;
using ShopIsDone.Arenas;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Arenas.Meddling;
using ShopIsDone.Entities.ParallelHunters;
using ShopIsDone.Utils;
using ShopIsDone.ClownRules;
using ShopIsDone.EntityStates;
using System.Linq;

namespace ShopIsDone.Actions
{
	public partial class ActionService : Node, IService, IInitializable
    {
        [Signal]
        public delegate void ActionStartedEventHandler(ArenaAction action);

        [Signal]
        public delegate void ActionFinishedEventHandler(ArenaAction action);

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private TileManager _TileManager;

        [Inject]
        private ArenaEntitiesService _ArenaEntities;

        [Export]
        private ScriptQueueService _ScriptQueueService;

        [Export]
        private ArenaOutcomeService _OutcomeService;

        [Export]
        private ActionMeddler _ActionMeddler;

        [Export]
        private ParallelHunterService _ParallelHunterService;

        [Export]
        private ClownRulesService _ClownRulesService;

        // State
        private ArenaAction _CurrentAction;
        private Dictionary<string, Variant> _CurrentMessage;

        public partial class ActionHistoryItem : GodotObject
        {
            public ArenaAction Action;
            public Dictionary<string, Variant> Message;
        }

        // For each turn
        public Array<ActionHistoryItem> ActionHistory { get { return _ActionHistory; } }
        private Array<ActionHistoryItem> _ActionHistory = new Array<ActionHistoryItem>();

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public Command ExecuteAction(ArenaAction action, Dictionary<string, Variant> message = null)
        {
            // Wrap in action meddler to interrupt any actions we want to meddle
            // with
            return _ActionMeddler.MeddleWithAction(
                action,
                message,
                new DeferredCommand(() => new SeriesCommand(
                    // Apply camera effects for the action
                    ApplyCameraFollow(action,
                        ApplyRotateToFaceEntity(action,
                            ApplyCameraZoom(action,
                                // Run parallel actions
                                _ParallelHunterService.RunParallelMoves(
                                    action,
                                    // Set current action 
                                    SetCurrentAction(
                                        action,
                                        message,
                                        action.Execute(message)
                                    )
                                )
                            )
                        )
                    ),
                    // Post action updates
                    new DeferredCommand(PostActionUpdate)
                ))
            );
        }

        // Usually run after an action, but can also be run after a significant
        // arena mutation whenever we want
        public Command PostActionUpdate()
        {
            // Wrap the whole thing in a Win-Fail check so we interrupt further
            // evaluation if the battle is over
            return WinFailCheck(new DeferredCommand(() => new SeriesCommand(
                // Update tiles
                new DeferredCommand(() => new ActionCommand(_TileManager.UpdateTiles)),
                // Update entities
                new DeferredCommand(UpdateEntities),
                // Idle all units
                new DeferredCommand(IdleAllUnits),

                // Process rules
                new DeferredCommand(() => new ConditionalCommand(
                    () => _CurrentAction != null,
                    new DeferredCommand(() =>
                        _ClownRulesService.ProcessActionRules(_CurrentAction, _CurrentMessage)
                    )
                )),

                // Run script queue
                new DeferredCommand(_ScriptQueueService.RunQueue),
                // Idle all units
                new DeferredCommand(IdleAllUnits)
            )));
        }

        private Command UpdateEntities()
        {
            return new SeriesCommand(
                _ArenaEntities
                    .GetAllArenaEntities()
                    .Where(e => e.IsInArena())
                    .Select(e => e.Update())
                    .ToArray()
                );
        }

        private Command IdleAllUnits()
        {
            return new SeriesCommand(_ArenaEntities
                .GetAllArenaEntities()
                .Where(u => u.HasComponent<EntityStateHandler>())
                .Select(u => u.GetComponent<EntityStateHandler>())
                .Select(state => state.RunIdleCurrentState())
                .ToArray()
            );
        }

        // Queries
        public bool HasCurrentAction()
        {
            return _CurrentAction != null;
        }

        public ArenaAction GetCurrentAction()
        {
            return _CurrentAction;
        }

        // Utilities
        private Command SetCurrentAction(ArenaAction action, Dictionary<string, Variant> message, Command actionCommand)
        {
            // Emit that the action has started
            EmitSignal(nameof(ActionStarted), action);

            // Connect to action command finished
            actionCommand.Connect(
                nameof(actionCommand.Finished),
                Callable.From(() => {
                    // Emit that the action is finished
                    EmitSignal(nameof(ActionFinished), action);
                }),
                (uint)ConnectFlags.OneShot
            );

            // Set the action and message
            _CurrentAction = action;
            _CurrentMessage = message;

            // Add it to the history
            _ActionHistory.Add(new ActionHistoryItem()
            {
                Action = _CurrentAction,
                Message = _CurrentMessage
            });

            // IF not a sub action
            if (action is not MoveSubAction)
            {
                // Connect to the current action finished with a cleanup function
                actionCommand.Connect(
                    nameof(actionCommand.Finished),
                    Callable.From(() => {
                        // Run final action finished hook (only for synchronous
                        // cleanup functions)
                        OnActionFinished();

                        // Clear out the current action / current message
                        _CurrentAction = null;
                        _CurrentMessage = null;
                    }),
                    (uint)ConnectFlags.OneShot
                );
            }

            // Pass through
            return actionCommand;
        }

        public void ResetActionHistory()
        {
            _ActionHistory.Clear();
        }

        private void OnActionFinished()
        {
            _ClownRulesService.ResetActionRules();
        }

        private Command WinFailCheck(Command next)
        {
            return new IfElseCommand(
                _OutcomeService.WasPlayerDefeated,
                new ActionCommand(_OutcomeService.AdvanceToDefeatPhase),
                new IfElseCommand(
                    _OutcomeService.IsPlayerVictorious,
                    new ActionCommand(_OutcomeService.AdvanceToVictoryPhase),
                    next
                )
            );
        }

        private Command ApplyRotateToFaceEntity(ArenaAction action, Command next)
        {
            // NB: Wrap in deferred call to calculate facing direction at execution
            // time, not call time
            return new DeferredCommand(() => new IfElseCommand(
                () => action.RotateToFaceEntity,
                _CameraService.RunRotateToFaceEntity(
                    action.Entity.FacingDirection,
                    next
                ),
                next
            ));
        }

        private Command ApplyCameraFollow(ArenaAction action, Command next)
        {
            return new SeriesCommand(
                new ConditionalCommand(
                    () => action.FollowEntity,
                    _CameraService.SetCameraTarget(action.Entity)
                ),
                next
            );
        }

        private Command ApplyCameraZoom(ArenaAction action, Command next)
        {
            return new IfElseCommand(
                () => action.FocusEntity,
                _CameraService.TemporaryCameraZoom(next),
                next
            );
        }
    }
}
