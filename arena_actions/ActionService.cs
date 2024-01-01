using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using Godot.Collections;
using System;
using ShopIsDone.Cameras;
using ShopIsDone.Tiles;

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

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public Command ExecuteAction(ArenaAction action, Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                ApplyCameraFollow(action,
                    ApplyCameraZoom(action,
                        action.Execute(message)
                    )
                ),
                // Post action updates
                new DeferredCommand(() => new ActionCommand(_TileManager.UpdateTiles))
            );
        }

        private Command ApplyCameraFollow(ArenaAction action, Command next)
        {
            return new IfElseCommand(
                () => action.FollowEntity,
                _CameraService.PanToTemporaryCameraTarget(action.Entity, next),
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
