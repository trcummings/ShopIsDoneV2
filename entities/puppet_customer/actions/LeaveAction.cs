using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.ActionPoints;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class LeaveAction : ArenaAction
    {
        private ActionPointHandler _ApHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _ApHandler = Entity.GetComponent<ActionPointHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return entity.HasComponent<ActionPointHandler>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            return new SeriesCommand(
                // Mark action as used
                base.Execute(message),
                // Die
                _ApHandler.Die(new ApDamagePayload() { Source = Entity })
            );
        }
    }
}

