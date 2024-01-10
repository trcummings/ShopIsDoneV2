using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.TriggerAreas
{
    // This version of the trigger area waits until a unit enters the trigger
    // area and idles to trigger
    [Tool]
    public partial class EntityTriggerArea : TriggerArea
	{
        [Inject]
        private ActionService _ActionService;

        private Callable _ActionFinished;
        private LevelEntity _Entity = null;
        private MoveSubAction _SubAction = null;

        public override void _Ready()
        {
            base._Ready();

            _ActionFinished = new Callable(this, nameof(OnActionFinished));

            BodyEntered += (Node3D node) =>
            {
                if (
                    // If we don't have one yet
                    //_Entity == null &&
                    // And it's an entity
                    node is LevelEntity entity
                )
                {
                    // Inject
                    InjectionProvider.Inject(this);

                    // If the action service has a current action, our entity is
                    // the one acting, and the action is a movement sub action, then
                    // connect to the action finished event
                    if (
                        _ActionService.HasCurrentAction() &&
                        _ActionService.GetCurrentAction() is MoveSubAction subAction &&
                        subAction.Entity == entity
                    )
                    {
                        _Entity = entity;
                        _SubAction = subAction;
                        _ActionService.Connect(nameof(_ActionService.ActionFinished), _ActionFinished);
                    }
                }
            };
        }

        private void OnActionFinished(ArenaAction action)
        {
            if (action == _SubAction)
            {
                // Trigger
                Trigger();

                // Disconnect
                _ActionService.Disconnect(nameof(_ActionService.ActionFinished), _ActionFinished);

                // Null out
                _Entity = null;
                _SubAction = null;
            }
        }
    }
}

