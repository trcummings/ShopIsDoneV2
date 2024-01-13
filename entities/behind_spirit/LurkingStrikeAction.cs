using Godot;
using Godot.Collections;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Positioning;
using System;
using ActionConsts = ShopIsDone.Actions.Consts;

namespace ShopIsDone.Entities.BehindSpirit.Actions
{
    public partial class LurkingStrikeAction : ArenaAction
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
            return entity.HasComponent<MicrogameHandler>();
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
            var positioning = (Positions)(int)message[ActionConsts.POSITIONING];
            var targetOutcomeHandler = target.GetComponent<IOutcomeHandler>();

            // Get customer's microgame
            var payload = new MicrogamePayload()
            {
                Targets = new[] { targetOutcomeHandler },
                Position = positioning,
                Message = message ?? new Dictionary<string, Variant>()
            };

            return new SeriesCommand(
                // Base call
                base.Execute(message),
                // Wait for emphasis
                new WaitForCommand(Entity, 0.5f),
                // Emerge
                _StateHandler.RunChangeState(Consts.States.EMERGE),
                // Wait for emphasis
                new WaitForCommand(Entity, 0.5f),
                // Run microgame
                _MicrogameHandler.RunMicrogame(payload),
                // Descend
                _StateHandler.RunChangeState(Consts.States.SINK)
            );
        }
    }
}

