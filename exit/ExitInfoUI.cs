using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Models;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using System.Linq;

namespace ShopIsDone.Exit
{
    public partial class ExitInfoUI : TargetInfoUI
    {
        [Inject]
        protected TileIndicator _TileIndicator;

        // Nodes
        private Label _OpenText;
        private Label _LockedText;

        private ModelComponent _Model;
        private ExitInteractionComponent _Exit;

        public override void _Ready()
        {
            // Ready nodes
            _OpenText = GetNode<Label>("%OpenText");
            _LockedText = GetNode<Label>("%LockedText");
        }

        protected override MoreInfoPayload GetInfoPayload()
        {
            return new MoreInfoPayload()
            {
                Title = _Entity.EntityName,
                Description = "A way out...?",
                Model = (IModel)(_Model.Model as Node3D).Duplicate(),
                Point = _Entity.WidgetPoint.Position with { Y = 0 }
            };
        }

        public override bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.HasComponent<ExitInteractionComponent>();
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            // Get exit component from interactable
            _Model = entity.GetComponent<ModelComponent>();
            _Exit = entity.GetComponent<ExitInteractionComponent>();

            // Set status
            if (_Exit.IsLocked)
            {
                _OpenText.Hide();
                _LockedText.Show();
            }
            else
            {
                _LockedText.Hide();
                _OpenText.Show();
            }
        }

        public override void ShowTileInfo()
        {
            // Show tile indicators for squares units can start the task on
            _TileIndicator.CreateIndicators(
                _Exit
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

