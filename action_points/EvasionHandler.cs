using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.ActionPoints
{
    public interface IEvasionHandler
    {
        bool EvadedDamage(LevelEntity source, Positions position);
        Command HandleEvasion(LevelEntity source, Positions position);
    }

    public partial class EvasionHandler : Node, IEvasionHandler
    {
        [Signal]
        public delegate void EvadedDebtDamageEventHandler();

        // NB: We can't dodge self-inflicted damage
        public virtual bool EvadedDamage(LevelEntity source, Positions position = Positions.Null)
        {
            return false;
        }

        public virtual Command HandleEvasion(LevelEntity source, Positions position = Positions.Null)
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

