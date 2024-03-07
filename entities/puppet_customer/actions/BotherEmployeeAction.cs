using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Core;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Positioning;
using Godot.Collections;
using ActionConsts = ShopIsDone.Actions.Consts;
using StateConsts = ShopIsDone.EntityStates.Consts;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using ShopIsDone.Models;

namespace ShopIsDone.Entities.PuppetCustomers.Actions
{
    public partial class BotherEmployeeAction : ArenaAction
    {
        private MicrogameHandler _MicrogameHandler;
        private ModelComponent _ModelComponent;

        [Inject]
        private EntityWidgetService _EntityWidgetService;

        public override void Init(ActionHandler actionHandler)
        {
            base.Init(actionHandler);
            _MicrogameHandler = Entity.GetComponent<MicrogameHandler>();
            _ModelComponent = Entity.GetComponent<ModelComponent>();
        }

        public override bool HasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<FacingDirectionHandler>() &&
                entity.HasComponent<MicrogameHandler>() &&
                entity.HasComponent<ModelComponent>();
        }

        public override bool TargetHasRequiredComponents(LevelEntity entity)
        {
            return
                entity.HasComponent<IOutcomeHandler>() &&
                entity.HasComponent<ModelComponent>();
        }

        public override Command Execute(Dictionary<string, Variant> message = null)
        {
            // Get the target from the message
            var target = (LevelEntity)message[ActionConsts.TARGET];
            var positioning = (Positions)(int)message[ActionConsts.POSITIONING];
            var targetModel = target.GetComponent<ModelComponent>();
            var targetOutcomeHandler = target.GetComponent<IOutcomeHandler>();

            // Get customer's microgame
            var payload = new MicrogamePayload()
            {
                Targets = new[] { targetOutcomeHandler },
                Position = positioning,
                Message = message ?? new Dictionary<string, Variant>()
            };

            return new SeriesCommand(
                // Mark action as used
                base.Execute(message),
                // Simultaneous bother / alert
                new ParallelCommand(
                    // Run bother animation
                    _ModelComponent.RunPerformAction(StateConsts.Customers.BOTHER),
                    // Show prompt popup
                    new AsyncCommand(() => _EntityWidgetService.PopupLabelAsync(
                        Entity.WidgetPoint,
                        _MicrogameHandler.GetMicrogamePrompt(payload))
                    ),
                    // Target alert popup
                    new AsyncCommand(() => _EntityWidgetService.AlertAsync(target.WidgetPoint)),
                    // Target alert animation
                    targetModel.RunPerformAction(StateConsts.ALERT)
                ),
                // Run employee microgame
                _MicrogameHandler.RunMicrogame(payload)
            );
        }
    }
}

