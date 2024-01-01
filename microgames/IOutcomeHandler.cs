using Godot;
using System;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Positioning;
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
        Command HandleOutcome(
            Microgame.Outcomes outcome,
            IOutcomeHandler[] targets,
            IOutcomeHandler source,
            Positions position
        );

        DamagePayload GetDamage();
    }
}
