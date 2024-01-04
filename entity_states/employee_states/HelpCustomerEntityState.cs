using System;
using Godot;
using ShopIsDone.EntityStates;
using Godot.Collections;

namespace ShopIsDone.Entities.Employees.States
{
	public partial class HelpCustomerEntityState : EntityState
    {
        [Signal]
        public delegate void RequestedPromptTextEventHandler(string promptText);

        [Export]
        private string _PromptText = "Can I help you?";

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            // If we have it, emit prompt text
            if (!string.IsNullOrEmpty(_PromptText))
            {
                EmitSignal(nameof(RequestedPromptText), _PromptText);
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

