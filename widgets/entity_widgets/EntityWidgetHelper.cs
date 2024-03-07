using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Widgets
{
    // Helper node for the EntityWidgetService under a level entity
    public partial class EntityWidgetHelper : Node
    {
        [Export]
        private NodePath _EntityPath;
        private LevelEntity _Entity;

        [Inject]
        private EntityWidgetService _WidgetService;

        public override void _Ready()
        {
            _Entity = GetNode<LevelEntity>(_EntityPath);
            _Entity.Connect(nameof(_Entity.Initialized), Callable.From(Init));
        }

        public void Init()
        {
            InjectionProvider.Inject(this);
        }

        public void PopupNumber(int amount, Color textColor, Color outlineColor)
        {
            _WidgetService.PopupNumber(_Entity.WidgetPoint, amount, textColor, outlineColor);
        }

        public void PopupLabel(string text)
        {
            _WidgetService.PopupLabel(_Entity.WidgetPoint, text);
        }
    }
}

