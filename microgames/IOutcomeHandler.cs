using Godot;
using System;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Core;
using Godot.Collections;

namespace ShopIsDone.Microgames.Outcomes
{
    public partial class DamagePayload : GodotObject
    {
        public int Damage;
        public LevelEntity Source;
        public Dictionary<string, Variant> Message = new Dictionary<string, Variant>();
    }

    public interface IOutcomeHandler : IComponent
    {
        public IDamageTarget DamageTarget { get; }

        Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload);

        Command BeforeOutcomeResolution(MicrogamePayload payload);

        Command AfterOutcomeResolution(MicrogamePayload payload);

        DamagePayload GetDamage(MicrogamePayload outcomePayload);
    }

    public interface IDamageTarget
    {
        bool CanReceiveDamage();

        Command ReceiveDamage(DamagePayload damage);
    }

    public class NullDamageTarget : IDamageTarget
    {
        public bool CanReceiveDamage()
        {
            return false;
        }

        public Command ReceiveDamage(DamagePayload _)
        {
            return new Command();
        }
    }
}
