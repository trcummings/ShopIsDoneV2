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

namespace ShopIsDone.Actions
{
	public partial class ActionService : Node, IService
    {
        [Signal]
        public delegate void ActionStartedEventHandler(ArenaAction action);

        [Signal]
        public delegate void ActionFinishedEventHandler(ArenaAction action);

        [Inject]
        private CameraService _CameraService;

        [Inject]
        private TileManager _TileManager;

        [Export]
        private ScriptQueueService _ScriptQueueService;

        [Export]
        private ArenaOutcomeService _OutcomeService;

        [Export]
        private ActionMeddler _ActionMeddler;

        [Export]
        private ParallelHunterService _ParallelHunterService;

        // State
        private ArenaAction _CurrentAction;
        private Dictionary<string, Variant> _CurrentMessage;

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

        public Command PostActionUpdate()
        {
            // Wrap the whole thing in a Win-Fail check so we interrupt further
            // evaluation if the battle is over
            return WinFailCheck(new DeferredCommand(() => new SeriesCommand(
                // Update tiles
                new DeferredCommand(() => new ActionCommand(_TileManager.UpdateTiles)),

                // TODO: Process rules

                // Run script queue
                new DeferredCommand(_ScriptQueueService.RunQueue)
            )));
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

            // IF not a sub action
            if (action is not MoveSubAction)
            {
                // Connect to the current action finished with a cleanup function
                actionCommand.Connect(
                    nameof(actionCommand.Finished),
                    Callable.From(() => {
                        _CurrentAction = null;
                        _CurrentMessage = null;
                    }),
                    (uint)ConnectFlags.OneShot
                );
            }

            // Pass through
            return actionCommand;
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
                _CameraService.RunRotateCameraTo(
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
