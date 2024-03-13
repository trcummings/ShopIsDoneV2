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
        public bool IsStackable = false;

        [Export]
        public int MaxStacks = 0;

        [Export]
        public int Duration = 0;

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

        public void StackEffect(StatusEffect effect)
        {
            // Ignore attempts to stack incompatible effects
            // NB: This should not happen but it doesn't hurt to guard it here
            if (effect.Id != Id) return;

            // FIXME: This doesn't really make sense unless all effects are
            // duration based for stacking, but YAGNI until they aren't
            Duration = Mathf.Min(Duration + effect.Duration, MaxStacks);
        }

        public virtual void ApplyEffect()
        {
            // Do nothing
        }

        public Command ProcessStatusEffect()
        {
            return new SeriesCommand(
                // Process the effect during the process phase
                OnProcessEffect(),
                // Handle duration check
                new IfElseCommand(
                    () => Duration == 0,
                    // If duration is down to zero, remove the effect
                    _Handler.RemoveEffect(Id),
                    // Otherwise, tick down duration
                    new ActionCommand(() =>
                    {
                        Duration = Mathf.Max(Duration - 1, 0);
                    })
                )
            );
        }

        public virtual void RemoveEffect()
        {
            // Do nothing
        }

        // API overrides for protection
        protected virtual Command OnProcessEffect()
        {
            return new Command();
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

