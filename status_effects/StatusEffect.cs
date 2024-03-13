using System;
using Godot;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.StatusEffects
{
    public partial class StatusEffect : Resource
    {
        protected StatusEffectHandler _Handler;

        [Export]
        public string Id;

        [Export]
        public string EffectName;

        [Export(PropertyHint.MultilineText)]
        public string EffectDescription;

        [Export]
        public Texture2D Icon;

        [Export]
        public bool IsDebuff = false;

        [ExportGroup("Stacking")]
        [Export]
        public StackingTypes EffectStackingType = StackingTypes.None;

        public enum StackingTypes
        {
            None,       // No stacking, only one instance of the effect can be active
            Duration,   // Stacking increases the duration
            Intensity,  // Stacking increases the intensity or potency of the effect
            Custom      // Custom behavior (requires overriding methods to implement)
        }

        // Starts with 1 as the initial application counts as the first stack
        [Export]
        public int CurrentStacks = 1;

        [Export]
        public int MaxStacks = 1;

        [ExportGroup("Duration")]
        [Export]
        public bool UsesDurationForRemoval = false;

        [Export]
        public int Duration = 0;

        [Export]
        public int MaxDuration = 100;

        [ExportGroup("Popup")]
        [Export(PropertyHint.Enum)]
        public Punctuations Punctuation = Punctuations.None;

        [Export]
        public bool ShowIconInPopup;

        public enum Punctuations
        {
            None,
            Exclamation,
            Question
        }

        [Inject]
        private ScriptQueueService _Queue;

        protected LevelEntity _Entity;

        public string GetPopupString()
        {
            return $"{EffectName}{PunctuationToString(Punctuation)}";
        }

        private static string PunctuationToString(Punctuations punctuation)
        {
            switch (punctuation)
            {
                case Punctuations.None: return "";
                case Punctuations.Exclamation: return "!";
                case Punctuations.Question: return "?";
                default: return "";
            }
        }

        /* Init method ONLY for the intialization of needed components */
        public virtual void Init(StatusEffectHandler handler)
        {
            _Handler = handler;
            _Entity = handler.Entity;
        }

        public bool IsStackable()
        {
            return EffectStackingType != StackingTypes.None;
        }

        public void StackEffect(StatusEffect effect)
        {
            // Ignore attempts to stack incompatible effects
            // NB: This should not happen but it doesn't hurt to guard it here
            if (effect.Id != Id || EffectStackingType == StackingTypes.None) return;

            switch (EffectStackingType)
            {
                case StackingTypes.Duration:
                    AdjustDuration(effect);
                    break;

                case StackingTypes.Intensity:
                    // Override this to adjust the intensity of the effect
                    AdjustIntensity(effect);
                    break;

                case StackingTypes.Custom:
                    // Override this for custom behavior
                    HandleCustomStacking(effect);
                    break;
            }
        }

        public virtual void ApplyEffect()
        {
            // Do nothing
        }

        /* Run any effects of the status effect here, e.g. how a Poison status 
         * effect would impart damage at the beginning of the turn */
        public virtual Command ProcessStatusEffect()
        {
            return new Command();
        }

        public Command TickEffectDuration()
        {
            return new IfElseCommand(
                // Check if the effect should be removed
                () => UsesDurationForRemoval && Duration == 0,
                // If duration is down to zero, remove the effect
                _Handler.RemoveEffect(Id),
                // Otherwise, tick down duration
                new ActionCommand(() =>
                {
                    if (UsesDurationForRemoval) Duration = Mathf.Max(Duration - 1, 0);
                })
            );
        }

        public virtual void RemoveEffect()
        {
            // Do nothing
        }

        protected virtual void HandleCustomStacking(StatusEffect effect)
        {
            // Override here
        }

        protected virtual void AdjustIntensity(StatusEffect effect)
        {
            // Increment stacks (for intensity) up to the max
            CurrentStacks = Mathf.Min(CurrentStacks + 1, MaxStacks);

            // Override for more
        }

        private void AdjustDuration(StatusEffect effect)
        {
            // Increment duration up to the max
            Duration = Mathf.Min(Duration + effect.Duration, MaxDuration);
        }

        #region Sandbox subclass methods
        /* Helper method to emit any command to the queue on triggering a 
         * passive effect */
        protected void EmitCommandToQueue(Command command)
        {
            _Queue.AddScriptToQueue(new CommandArenaScript()
            {
                CommandFn = () => command
            });
        }
        #endregion
    }
}

