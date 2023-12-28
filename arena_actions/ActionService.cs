using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Cameras;

namespace ShopIsDone.Actions
{
	public partial class ActionService : Node, IService
    {
        [Signal]
        public delegate void ActionFinishedEventHandler();

        [Inject]
        private CameraService _CameraService;

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public Command ExecuteAction(ArenaAction action, Dictionary<string, Variant> message = null)
        {
            return ApplyCameraFollow(action,
                ApplyCameraZoom(action, action.Execute(message))
            );
        }

        private Command ApplyCameraFollow(ArenaAction action, Command next)
        {
            return new IfElseCommand(
                () => action.FollowEntity,
                _CameraService.PanToTemporaryCameraTarget(next),
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
