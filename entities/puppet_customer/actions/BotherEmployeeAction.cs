using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Positioning;
using Godot.Collections;
using ActionConsts = ShopIsDone.Actions.Consts;
using ShopIsDone.Entities.PuppetCustomers.States;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class BotherEmployeeAction : ArenaAction
    {
        private EntityStateHandler _StateHandler;
        private MicrogameHandler _MicrogameHandler;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _StateHandler = Entity.GetComponent<EntityStateHandler>();
            _MicrogameHandler = Entity.GetComponent<MicrogameHandler>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<FacingDirectionHandler>() &&
                entity.HasComponent<MicrogameHandler>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<IOutcomeHandler>() &&
                entity.HasComponent<EntityStateHandler>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the target from the message
            var target = (LevelEntity)message[ActionConsts.TARGET];
            var targetStateHandler = target.GetComponent<EntityStateHandler>();
            var targetOutcomeHandler = target.GetComponent<IOutcomeHandler>();

            // Get customer's microgame
            var payload = new MicrogamePayload()
            {
                Targets = new[] { targetOutcomeHandler },
                Position = Positioning.GetPositioning(Entity.FacingDirection, target.FacingDirection),
                Message = message ?? new Dictionary<string, Variant>()
            };

            return new SeriesCommand(
                // Mark action as used
                base.Execute(message),
                // Simultaneous bother / alert
                new ParallelCommand(
                    // Run bother animation
                    _StateHandler.RunChangeState("bother", new Dictionary<string, Variant>
                    {
                        { BotherEntityState.PROMPT_TEXT, _MicrogameHandler.GetMicrogamePrompt(payload) }
                    }),
                    // Alert Target
                    targetStateHandler.RunChangeState("alert")
                ),
                // Run employee microgame
                _MicrogameHandler.RunMicrogame(payload)
            );
        }
    }
}

