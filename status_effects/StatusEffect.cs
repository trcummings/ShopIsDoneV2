using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.StatusEffects
{
    public partial class StatusEffect : Resource
    {
        protected StatusEffectHandler _Handler;

        [Export]
        public string Id;

        [Export]
        public string EffectName;

        [Export]
        public bool Stackable = false;

        [Export]
        public int MaxStacks = 0;

        [Export]
        public int Duration = 0;

        protected LevelEntity _Entity;

        /* Init method ONLY for the intialization of needed components */
        public virtual void Init(StatusEffectHandler handler)
        {
            _Handler = handler;
            _Entity = handler.Entity;
        }

        public virtual Command ApplyEffect()
        {
            return new Command();
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
                    _Handler.RemoveEffect(this),
                    // Otherwise, tick down duration
                    new ActionCommand(() =>
                    {
                        Duration = Mathf.Max(Duration - 1, 0);
                    })
                )
            );
        }

        public virtual Command RemoveEffect()
        {
            return new Command();
        }

        // API overrides for protection
        protected virtual Command OnProcessEffect()
        {
            return new Command();
        }
    }
}

