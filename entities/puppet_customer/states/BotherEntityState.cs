using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Models;
using Godot.Collections;

namespace ShopIsDone.Entities.PuppetCustomers.States
{
    public partial class BotherEntityState : EntityState
    {
        [Signal]
        public delegate void RequestedPromptTextEventHandler(string promptText);

        [Export]
        private Model _Model;

        private Callable _BotherCallback;

        public const string PROMPT_TEXT = "PromptText";

        public override void _Ready()
        {
            base._Ready();
            _BotherCallback = new Callable(this, nameof(OnAnimFinished));
        }

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            // Pull the prompt text from the message
            var promptText = (string)message?[PROMPT_TEXT] ?? "";
            if (!string.IsNullOrEmpty(promptText)) EmitSignal(nameof(RequestedPromptText), promptText);

            Task _ = _Model.PerformAnimation("bother");
            // Connect to a bother callback that will end the state at the peak
            // of the animation rather than after it's finished
            _Model.Connect(nameof(_Model.AnimationFinished), _BotherCallback);
        }

        private void OnAnimFinished(string eventName)
        {
            // Disconnect
            _Model.Disconnect(nameof(_Model.AnimationFinished), _BotherCallback);
            base.Enter();
        }

        public override bool IsInArena()
        {
            return true;
        }

        public override bool CanAct()
        {
            return true;
        }
    }
}

