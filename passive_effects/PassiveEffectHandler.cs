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

        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public override void Init()
        {
            // Initialize each effect
            var clonedList = _Effects.ToList();
            _Effects.Clear();
            foreach (var effect in clonedList) ApplyEffect(effect);
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
            InitEffect(effect);
            GetEffect(effect.Id)?.ApplyEffect();
        }

        public void RemoveEffect(PassiveEffect effect)
        {
            // Remove from list
            _Effects.Remove(effect);
            // Run removal hook
            effect.RemoveEffect();
        }

        private void InitEffect(PassiveEffect effect)
        {
            // Duplicate effect
            var newEffect = (PassiveEffect)effect.Duplicate();
            // Add to list
            _Effects.Add(newEffect);
            // Inject into action
            _InjectionProvider.InjectObject(newEffect);
            // Init object
            newEffect.Init(this);
        }

    }
}
