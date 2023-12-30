using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Tiles;
using System;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class BotherEmployeeAction : ArenaAction
    {
        private EntityStateHandler _StateHandler;
        private FacingDirectionHandler _FacingDirectionHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
            _FacingDirectionHandler = Entity.GetComponent<FacingDirectionHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<FacingDirectionHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<FacingDirectionHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }
    }
}

