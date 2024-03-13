using Godot;
using ShopIsDone.Core;
using ShopIsDone.Microgames;
using ShopIsDone.Microgames.Outcomes;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.ClownPuppet
{
    public partial class ClownPuppetOutcomeHandler : OutcomeHandler
    {
        public override void Init()
        {
            base.Init();
            InjectionProvider.Inject(this);
        }

        public override Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return target.ReceiveDamage(GetDamage(outcomePayload));
        }

        public override DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            return new DamagePayload()
            {
                Damage = outcomePayload.WonMicrogame() ? 3 : 0,
                Source = Entity
            };
        }
    }
}