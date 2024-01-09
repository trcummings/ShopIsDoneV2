using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Cameras;
using ShopIsDone.Tiles;
using ShopIsDone.Arenas;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Arenas.Meddling;

namespace ShopIsDone.Actions
{
	public partial class ActionService : Node, IService
    {
        [Signal]
        public delegate void ActionFinishedEventHandler();

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
                                action.Execute(message)
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
            return new IfElseCommand(
                () => action.RotateToFaceEntity,
                _CameraService.RunRotateCameraTo(
                    action.Entity.FacingDirection.Reflect(Vector3.Up),
                    next
                ),
                next
            );
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
