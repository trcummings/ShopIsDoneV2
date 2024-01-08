using Godot;
using System;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Core;

namespace ShopIsDone.Microgames.Outcomes
{
    public class DamagePayload
    {
        public int Health;
        public int Damage;
        public int Drain;
        public int Defense;
        public int DrainDefense;
        public int Piercing;
    }

    public interface IOutcomeHandler : IComponent
    {
        Command HandleOutcome(MicrogamePayload payload);

        DamagePayload GetDamage();
    }
}
