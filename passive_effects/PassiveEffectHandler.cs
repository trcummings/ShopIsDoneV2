using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Linq;

namespace ShopIsDone.PassiveEffects
{
    public partial class PassiveEffectHandler : NodeComponent
    {
        [Export]
        private Array<PassiveEffect> _Effects = new Array<PassiveEffect>();
        public Array<PassiveEffect> Effects { get { return _Effects; } }

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public override void Init()
        {
            // Inject
            _InjectionProvider.InjectObject(this);

            // Initialize each effect
            var clonedList = _Effects.ToList();
            // Clear the initial list of effects
            _Effects.Clear();
            // Apply and init each effect
            foreach (var effect in clonedList)
            {
                var newEffect = InitEffect(effect);
                _Effects.Add(newEffect);
                newEffect.ApplyEffect();
            }
        }

        public bool HasEffect(string id)
        {
            return _Effects.Any(se => se.Id == id);
        }

        public PassiveEffect GetEffect(string id)
        {
            return _Effects.ToList().Find(se => se.Id == id);
        }

        public void ApplyEffect(PassiveEffect effect)
        {
            // We can't add it twice
            if (HasEffect(effect.Id)) return;

            // Initialize, apply it, and add it to the effects
            var newEffect = InitEffect(effect);
            _Effects.Add(newEffect);
            newEffect.ApplyEffect();

        }

        public void RemoveEffect(PassiveEffect effect)
        {
            // Remove from list
            _Effects.Remove(effect);
            // Run removal hook
            effect.RemoveEffect();
        }

        public void ProcessEffects()
        {
            foreach (var effect in _Effects) effect.ProcessEffect();
        }

        private PassiveEffect InitEffect(PassiveEffect effect)
        {
            // Duplicate effect
            var newEffect = (PassiveEffect)effect.Duplicate();
            // Inject into action
            _InjectionProvider.InjectObject(newEffect);
            // Init object
            newEffect.Init(this);

            return newEffect;
        }
    }
}
