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
        private Array<StatusEffect> _Effects = new Array<StatusEffect>();
        private InjectionProvider _InjectionProvider;

        public override void _Ready()
        {
            base._Ready();
            _InjectionProvider = InjectionProvider.GetProvider(this);
        }

        public override void Init()
        {

        }

        public bool HasStatusEffect(string id)
        {
            return _Effects.Any(se => se.Id == id);
        }

        public Command AddStatusEffect(StatusEffect status)
        {
            // Return command
            return new SeriesCommand(

            );
        }

        public Command RemoveStatusEffect(StatusEffect status)
        {
            return new SeriesCommand(

            );
        }

        public Command ProcessStatusEffects()
        {
            return new SeriesCommand(
                _Effects.Select(e => e.ProcessStatusEffect())
                .ToArray()
            );
        }
    }
}
