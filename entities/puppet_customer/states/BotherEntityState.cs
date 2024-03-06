using System;
using Godot;
using ShopIsDone.EntityStates;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;

namespace ShopIsDone.Entities.PuppetCustomers.States
{
    public partial class BotherEntityState : AnimatedEntityState
    {
        [Inject]
        private EntityWidgetService _WidgetService;

        public const string PROMPT_TEXT = "PromptText";

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            // Pull the prompt text from the message
            var promptText = (string)message?[PROMPT_TEXT] ?? "";
            if (!string.IsNullOrEmpty(promptText))
            {
                _WidgetService.PopupLabel(_Entity.WidgetPoint, promptText);
            }

            // Run the enter animation
            RunEnterAnimation();
        }
    }
}

