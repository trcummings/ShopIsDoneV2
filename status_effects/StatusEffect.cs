using System;
using Godot;
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

        public virtual void Init(StatusEffectHandler handler)
        {
            _Handler = handler;
        }

        public virtual Command OnAddEffect()
        {
            return new Command();
        }

        public virtual Command ProcessStatusEffect()
        {
            return new Command();
        }

        public virtual Command OnRemoveEffect()
        {
            return new Command();
        }
    }
}

