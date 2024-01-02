using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Models;
using Godot.Collections;

namespace ShopIsDone.Entities.PuppetCustomers.States
{
    public partial class BotherEntityState : AnimatedEntityState
    {
        [Signal]
        public delegate void RequestedPromptTextEventHandler(string promptText);

        public const string PROMPT_TEXT = "PromptText";

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            // Pull the prompt text from the message
            var promptText = (string)message?[PROMPT_TEXT] ?? "";
            if (!string.IsNullOrEmpty(promptText)) EmitSignal(nameof(RequestedPromptText), promptText);

            // Run the enter animation
            RunEnterAnimation();
        }
    }
}

