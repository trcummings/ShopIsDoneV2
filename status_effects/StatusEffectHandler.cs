using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Widgets;
using System;
using System.Linq;

namespace ShopIsDone.StatusEffects
{
    public partial class StatusEffectHandler : NodeComponent
    {
        [Signal]
        public delegate void DebuffAppliedEventHandler();

        [Signal]
        public delegate void BuffAppliedEventHandler();

        [Export]
        private Array<StatusEffect> _Effects = new Array<StatusEffect>();
        public Array<StatusEffect> Effects { get { return _Effects; } }

        [Inject]
        private EntityWidgetService _WidgetService;

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
            return new IfElseCommand(
                // If we have it already, attempt to stack the effect
                () => HasStatusEffect(effect.Id),
                // Check for stacking
                new ConditionalCommand(
                    effect.IsStackable,
                    new ActionCommand(() =>
                    {
                        // NB: We don't need to null check this
                        var baseEffect = GetEffect(effect.Id);
                        baseEffect.StackEffect(effect);
                        // TODO: Global UI signal
                    })
                ),
                // Otherwise, apply it normally
                new DeferredCommand(() =>
                {
                    // Initialize, apply it, and add it to the effects
                    var newEffect = InitEffect(effect);
                    _Effects.Add(newEffect);
                    newEffect.ApplyEffect();

                    // TODO: Global UI signal

                    // Run the application animations / FX
                    return new SeriesCommand(
                        new ActionCommand(() => {
                            if (newEffect.IsDebuff) EmitSignal(nameof(DebuffApplied));
                            else EmitSignal(nameof(BuffApplied));
                        }),
                        new AsyncCommand(() =>
                            _WidgetService.PopupLabelAsync(
                                Entity.WidgetPoint,
                                newEffect.GetPopupString()
                            )
                        ),
                        new WaitForCommand(this, 0.25f)
                    );
                })
            );
        }

        public Command RemoveEffect(string id)
        {
            // If we have it, then simply remove it and call the removal hook
            return new ConditionalCommand(
                () => HasStatusEffect(id),
                new ActionCommand(() =>
                {
                    var effect = GetEffect(id);
                    _Effects.Remove(effect);
                    effect.RemoveEffect();
                })
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

        public Command TickEffectDurations()
        {
            return new SeriesCommand(
                _Effects
                    .Select(e => e.TickEffectDuration())
                    .ToArray()
            );
        }

        private StatusEffect InitEffect(StatusEffect effect)
        {
            // Duplicate effect
            var newEffect = (StatusEffect)effect.Duplicate();
            // Inject into action
            _InjectionProvider.InjectObject(newEffect);
            // Init object
            newEffect.Init(this);

            return newEffect;
        }
    }
}
