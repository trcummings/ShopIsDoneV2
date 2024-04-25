using Godot;
using System.Linq;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using ShopIsDone.Models;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;

namespace ShopIsDone.Lighting.UI
{
    public partial class LightSwitchInfoUI : TargetInfoUI
    {
        [Inject]
        protected TileIndicator _TileIndicator;

        // Nodes
        private Label _SwitchStatus;

        // Components
        private LightSwitchComponent _LightSwitch;
        private LightSwitchInteractionComponent _InteractionComponent;
        private ModelComponent _Model;

        public override void _Ready()
        {
            // Ready nodes
            _SwitchStatus = GetNode<Label>("%SwitchStatus");
        }

        public override bool IsValidEntityForUI(LevelEntity entity)
        {
            return
                entity.HasComponent<LightSwitchComponent>() &&
                entity.HasComponent<LightSwitchInteractionComponent>() &&
                entity.IsActive();
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            // Get components
            _LightSwitch = entity.GetComponent<LightSwitchComponent>();
            _InteractionComponent = entity.GetComponent<LightSwitchInteractionComponent>();
            _Model = entity.GetComponent<ModelComponent>();

            // Set switch status
            _SwitchStatus.Text = _LightSwitch.IsFlippedOn ? "On" : "Off";
        }

        protected override MoreInfoPayload GetInfoPayload()
        {
            return new MoreInfoPayload()
            {
                Title = _Entity.EntityName,
                Description = "Are you sure you should be touching that?",
                Model = (IModel)(_Model.Model as Node3D).Duplicate(),
                Point = _Entity.WidgetPoint.Position with { Y = 0 }
            };
        }

        public override void ShowTileInfo()
        {
            // Show tile indicators for squares units can start the task on
            _TileIndicator.CreateIndicators(
                _InteractionComponent
                    .GetSelectTiles()
                    .Where(t => t.IsLit())
                    .Select(t => t.GlobalPosition),
                TileIndicator.IndicatorColor.Yellow
            );
        }

        public override void CleanUp()
        {
            base.CleanUp();
            // Clear away tile indicators
            _TileIndicator.ClearIndicators();
        }
    }
}

