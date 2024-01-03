using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IEvasionHandler
    {
        bool EvadedDamage(ApDamagePayload payload);

        Command HandleEvasion(ApDamagePayload payload);
    }

    public partial class EvasionHandler : Node, IEvasionHandler
    {
        [Signal]
        public delegate void EvadedDebtDamageEventHandler();

        // NB: We can't dodge self-inflicted damage
        public virtual bool EvadedDamage(ApDamagePayload payload)
        {
            return false;
        }

        public virtual Command HandleEvasion(ApDamagePayload payload)
        {
            return new Command();
        }


        //// Evasion Check
        //private bool EvadedDamage(LevelEntity source, float positioningHitChance)
        //{
        //    // If we're the source, no evasion check
        //    if (source == Entity) return false;

        //    // Get our dodge threshold from the positioning weight
        //    var threshold = Mathf.Max(1f - positioningHitChance, 0f);

        //    // Get a random float between 1 and 0
        //    var randResult = _RNG.RandfRange(0.01f, 0.99f);

        //    // If our threshold is greater than the result, it's a dodge
        //    return threshold > randResult;
        //}
    }
}

