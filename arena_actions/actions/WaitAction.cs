using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Actions
{
    public partial class WaitAction : ArenaAction
    {
        private EntityStateHandler _StateHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<EntityStateHandler>();
        }

        // Visible in menu if we're in the idle state, as most actions are
        public override bool IsVisibleInMenu()
        {
            return _StateHandler.IsInState(StateConsts.IDLE);
        }
    }
}