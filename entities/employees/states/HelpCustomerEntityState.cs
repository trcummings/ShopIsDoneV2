using System;
using Godot;
using ShopIsDone.EntityStates;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;

namespace ShopIsDone.Entities.Employees.States
{
	public partial class HelpCustomerEntityState : EntityState
    {
        [Export]
        private string _PromptText = "Can I help you?";

        [Inject]
        private EntityWidgetService _WidgetService;

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            // If we have it, emit prompt text
            if (!string.IsNullOrEmpty(_PromptText))
            {
                _WidgetService.PopupLabel(_Entity.WidgetPoint, _PromptText);
                GetTree()
                    .CreateTimer(0.5f)
                    .Connect(
                        "timeout",
                        Callable.From(() => EmitSignal(nameof(StateEntered))),
                        (uint)ConnectFlags.OneShot
                    );
            }
            // Otherwise, just complete state enter
            else base.Enter();
        }
    }
}

