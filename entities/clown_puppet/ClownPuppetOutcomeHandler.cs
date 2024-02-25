using Godot;
using ShopIsDone.Core;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.ClownPuppet
{
    public partial class ClownPuppetOutcomeHandler : NodeComponent, IOutcomeHandler
    {
        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        // Empty outcome, only deals damage
        public Command HandleOutcome(MicrogamePayload payload)
        {
            return new Command();
        }

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = 0,
                Defense = 0,
                DrainDefense = 0,
                Damage = 2,
                Drain = 0,
                Piercing = 0
            };
        }
    }
}