using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class IntimidateAction : ArenaAction
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

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                base.Execute(message),
                // Push intimidate state
                new AwaitSignalCommand(
                    _StateHandler,
                    nameof(_StateHandler.PushedState),
                    nameof(_StateHandler.PushState),
                    "intimidate"
                )
            );
        }
    }
}