using System;
using ShopIsDone.ActionPoints;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ApConsts = ShopIsDone.ActionPoints.Consts;

namespace ShopIsDone.Microgames.Outcomes
{
    public partial class EmployeeOutcomeHandler : NodeComponent, IOutcomeHandler
    {
        [Export]
        private ActionPointHandler _ActionPointHandler;

        public Command HandleOutcome(MicrogamePayload payload)
        {
            var damage = payload.Source.GetDamage();

            // Concat payload message to damage payload
            var message = new Dictionary<string, Variant>()
            {
                { ApConsts.DAMAGE_SOURCE, payload.Source.Entity },
                // If it's a win (for the player) pass along no damage
                { ApConsts.DEBT_DAMAGE, payload.Outcome == Microgame.Outcomes.Win ? 0 : damage.Damage }
            };
            if (payload.Message != null)
            {
                foreach (var kv in payload.Message) message[kv.Key] = kv.Value;
            }

            return new ConditionalCommand(
                // Are we still active check
                Entity.IsActive,
                // Deal damage to ourselves
                _ActionPointHandler.TakeAPDamage(message)
            );
        }

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = _ActionPointHandler.ActionPoints,
                Defense = _ActionPointHandler.DebtGuard,
                DrainDefense = _ActionPointHandler.DrainGuard,
                Damage = 1,
                Drain = 0,
                Piercing = 0,
            };
        }
    }
}

