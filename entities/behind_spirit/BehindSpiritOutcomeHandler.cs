using System;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Microgames;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;

namespace ShopIsDone.Entities.BehindSpirit
{
    public partial class BehindSpiritOutcomeHandler : NodeComponent, IOutcomeHandler
    {
        // Empty outcome
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
                Damage = 3,
                Drain = 0,
                Piercing = 0
            };
        }
    }
}

