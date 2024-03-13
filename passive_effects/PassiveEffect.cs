using System;
using Godot;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Core;
using ShopIsDone.Levels;
using ShopIsDone.StatusEffects;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.PassiveEffects
{
    public partial class PassiveEffect : Resource
    {
        [Export]
        public string Id;

        [Export]
        public string EffectName;

        [Export(PropertyHint.MultilineText)]
        public string EffectDescription;

        [Export]
        public Texture2D Icon;

        [Inject]
        private ScriptQueueService _Queue;

        [Inject]
        private LevelRngService _RngService;

        protected PassiveEffectHandler _Handler;
        protected LevelEntity _Entity;
        private StatusEffectHandler _StatusHandler;

        /* Init method ONLY for the intialization of needed components */
        public virtual void Init(PassiveEffectHandler handler)
        {
            _Handler = handler;
            _Entity = handler.Entity;
            _StatusHandler = _Entity.GetComponent<StatusEffectHandler>();
        }

        public virtual void ApplyEffect()
        {
            // Do nothing
        }

        public virtual void RemoveEffect()
        {
            // Do nothing
        }

        /* Gets called once per turn on prep player turn phase, allowing state 
         * updates per turn / resets */
        public virtual void ProcessEffect()
        {
            // Do nothing
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

        protected bool HasStatusEffect(string id)
        {
            return _StatusHandler.HasStatusEffect(id);
        }

        protected Command ApplyStatusEffect(StatusEffect effect)
        {
            return _StatusHandler.ApplyEffect(effect);
        }

        protected bool PercentCheck(float value)
        {
            return _RngService.PercentCheck(value);
        }
        #endregion
    }
}

