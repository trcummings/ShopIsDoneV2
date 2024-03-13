using System;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Microgames.Outcomes
{
	public partial class OutcomeHandler : NodeComponent, IOutcomeHandler
	{
        public IDamageTarget DamageTarget { get { return GetDamageTarget(); } }

        public virtual Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return new Command();
        }

        /* This is a hook to be run right before the resolution of the outcome */
        public virtual Command BeforeOutcomeResolution(MicrogamePayload payload)
        {
            return new Command();
        }

        public virtual Command AfterOutcomeResolution(MicrogamePayload payload)
        {
            return new Command();
        }

        protected virtual IDamageTarget GetDamageTarget()
        {
            return new NullDamageTarget();
        }

        public virtual DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            return new DamagePayload();
        }
    }
}

