using Godot;
using System;
using ShopIsDone.Core;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Models;

namespace ShopIsDone.Exit
{
    public partial class ExitInfoUI : TargetInfoUI
    {
        // Nodes
        private Label _OpenText;
        private Label _LockedText;

        private ModelComponent _Model;

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

            // Get exit component from interactable
            _Model = entity.GetComponent<ModelComponent>();
            var exit = entity.GetComponent<ExitInteractionComponent>();

            // Set status
            if (exit.IsLocked)
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
    }
}

