using System;
using ShopIsDone.ActionPoints;
using Godot;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Commands;
using ApConsts = ShopIsDone.ActionPoints.Consts;
using ShopIsDone.Microgames;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerOutcomeHandler : OutcomeHandler
    {
        [Export]
        private ActionPointHandler _ActionPointHandler;

        public override Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return target.ReceiveDamage(GetDamage(outcomePayload));
        }

        public override DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            // Copy over the message to the damage payload's message
            var message = new Dictionary<string, Variant>();
            foreach (var kv in outcomePayload?.Message ?? new Dictionary<string, Variant>())
            {
                message[kv.Key] = kv.Value;
            }

            return new DamagePayload()
            {
                // If we won, pass along full damage
                Damage = outcomePayload.WonMicrogame() ? 1 : 0,
                Source = Entity,
                Message = message
            };
        }

        protected override IDamageTarget GetDamageTarget()
        {
            return new ActionPointDamageTarget()
            {
                Entity = Entity,
                ApHandler = _ActionPointHandler
            };
        }
    }
}

