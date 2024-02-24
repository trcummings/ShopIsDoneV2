using System;
using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using Godot.Collections;
using ShopIsDone.Utils.Positioning;
using ActionConsts = ShopIsDone.Actions.Consts;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Entities.Employees.Actions
{
	public partial class HelpCustomerAction : ArenaAction
    {
        private IOutcomeHandler _OutcomeHandler;
        private EntityStateHandler _StateHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _OutcomeHandler = Entity.GetComponent<IOutcomeHandler>();
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<EntityStateHandler>() &&
                entity.HasComponent<IOutcomeHandler>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<MicrogameHandler>() &&
                entity.HasComponent<IOutcomeHandler>();
        }

        // Visible in menu if we're in the idle state, as most actions are
        public override bool IsVisibleInMenu()
        {
            return _StateHandler.IsInState(StateConsts.IDLE);
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the target from the message
            var target = (LevelEntity)message[ActionConsts.TARGET];
            var positioning = (Positions)(int)message[ActionConsts.POSITIONING];
            var targetMicrogameHandler = target.GetComponent<MicrogameHandler>();

            // Get customer's microgame
            var payload = new MicrogamePayload()
            {
                Targets = new[] { _OutcomeHandler },
                Position = positioning,
                Message = message ?? new Dictionary<string, Variant>()
            };

            return new SeriesCommand(
                // Mark action as used
                base.Execute(message),
                // Run help customer state change
                _StateHandler.RunChangeState(StateConsts.Employees.HELP_CUSTOMER),
                // Run employee microgame
                targetMicrogameHandler.RunMicrogame(payload),
                // If employee is still active, then return to idle
                new ConditionalCommand(
                    Entity.IsActive,
                    _StateHandler.RunChangeState(StateConsts.IDLE)
                )
            );
        }
    }
}

