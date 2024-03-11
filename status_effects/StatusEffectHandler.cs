using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Linq;

namespace ShopIsDone.StatusEffects
{
    public partial class StatusEffectHandler : NodeComponent
    {
        [Export]
        private Array<StatusEffect> _Effects = new Array<StatusEffect>();
        public Array<StatusEffect> Effects { get { return _Effects; } }

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
            // Clear the initial list of effects
            _Effects.Clear();
            // Apply and init each effect
            foreach (var effect in clonedList) ApplyEffect(effect).Execute();
        }

        public bool HasStatusEffect(string id)
        {
            return _Effects.Any(se => se.Id == id);
        }

        public StatusEffect GetEffect(string id)
        {
            return _Effects.ToList().Find(se => se.Id == id);
        }

        public Command ApplyEffect(StatusEffect effect)
        {
            return new SeriesCommand(
                new ActionCommand(() => InitEffect(effect)),
                new DeferredCommand(() =>
                    GetEffect(effect.Id)?.ApplyEffect() ??
                    new Command()
                )
            );
        }

        public Command RemoveEffect(StatusEffect effect)
        {
            return new SeriesCommand(
                new ActionCommand(() => _Effects.Remove(effect)),
                new DeferredCommand(effect.RemoveEffect)
            );
        }

        public Command ProcessEffects()
        {
            return new SeriesCommand(
                _Effects
                    .Select(e => e.ProcessStatusEffect())
                    .ToArray()
            );
        }

        private void InitEffect(StatusEffect effect)
        {
            // Duplicate effect
            var newEffect = (StatusEffect)effect.Duplicate();
            // Add to list
            _Effects.Add(newEffect);
            // Inject into action
            _InjectionProvider.InjectObject(newEffect);
            // Init object
            newEffect.Init(this);
        }
    }
}
