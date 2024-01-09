using Godot;
using Godot.Collections;
using ShopIsDone.EntityStates;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.ArenaInteractions;

namespace ShopIsDone.Actions
{
    public partial class InteractAction : ArenaAction
    {
        [Inject]
        private ActionService _ActionService;

        private EntityStateHandler _StateHandler;
        private UnitInteractionHandler _InteractionHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
            _InteractionHandler = Entity.GetComponent<UnitInteractionHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<EntityStateHandler>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<InteractionComponent>();
        }

        public override bool IsAvailable()
        {
            if (!base.IsAvailable()) return false;

            // If we don't have any interactions in range, we can't use this
            // action
            return _InteractionHandler.GetInteractionsInRange().Count > 0;
        }

        // Visible in menu if we're in the idle state, as most actions are
        public override bool IsVisibleInMenu()
        {
            return _StateHandler.IsInState(StateConsts.IDLE);
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new Command();
        }
    }
}