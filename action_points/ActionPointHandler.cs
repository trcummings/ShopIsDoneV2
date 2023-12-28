using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.ActionPoints
{
    // This is a component that handles damage, debt, guard, and death
    public partial class ActionPointHandlerComponent : NodeComponent
    {
        [Signal]
        public delegate void SpentApEventHandler(int amount);

        [Signal]
        public delegate void RecoveredApEventHandler(int amount);

        [Signal]
        public delegate void HealedDebtEventHandler(int amount);

        [Signal]
        public delegate void ReceivedExcessApEventHandler(int amount);

        [Signal]
        public delegate void SpentExcessApEventHandler(int amount);

        [Signal]
        public delegate void TookDebtDamageEventHandler(int amount);

        [Signal]
        public delegate void TookDebtFromDirectionEventHandler(Vector3 source);

        [Signal]
        public delegate void TookApDrainEventHandler(int amount);

        [Signal]
        public delegate void EvadedDebtDamageEventHandler(Vector3 from);

        [Signal]
        public delegate void MaxedOutDebtEventHandler();

        [Signal]
        public delegate void TookExternalDamageEventHandler(LevelEntity source, int amount);

        // Damage
        [Export]
        public int DrainGuard = 0;

        [Export]
        public int DebtGuard = 0;

        [Export]
        public int ActionPoints = 5;

        [Export]
        public int ActionPointDebt = 0;

        [Export]
        public int ActionPointExcess = 0;

        [Export]
        public int MaxActionPoints = 5;

        // FIXME: Pull from a RNG singleton that loads in from a seed saved in the level data so there's no
        // save scumming allowed 
        private RandomNumberGenerator _RNG = new RandomNumberGenerator();

        public override void _Ready()
        {
            _RNG.Randomize();
        }

        // Public API
        public virtual bool HasEnoughAPForAction(int amount)
        {
            return ActionPoints >= amount;
        }

        public bool IsMaxedOut()
        {
            return ActionPointDebt >= MaxActionPoints;
        }

        public virtual void TakeAPDamage(Dictionary<string, Variant> message = null)
        {
            // Get source of damage
            var source = (LevelEntity)message.GetValueOrDefault(Consts.DAMAGE_SOURCE);

            // Get bools for decision making
            bool receivedApDrain = message.ContainsKey(Consts.DAMAGE_AMOUNT);
            bool receivedDirectApDebt = message.ContainsKey(Consts.DEBT_DAMAGE);

            // Get drain and direct debt damage from message
            var drainAmount = (int)message.GetValueOrDefault(Consts.DAMAGE_AMOUNT, 0);
            var directDebtDamage = (int)message.GetValueOrDefault(Consts.DEBT_DAMAGE, 0);

            // Calculate totals based on spillover and guard
            var totalDrain = Math.Max(drainAmount - DrainGuard, 0);
            var apAfterDrain = ActionPoints - totalDrain;
            // Total debt damage should be a positive number
            var totalDebtDamage = Mathf.Max(
                (apAfterDrain < 0
                    // AP after drain here is negative, so use its absolute value
                    ? directDebtDamage + Mathf.Abs(apAfterDrain)
                    : directDebtDamage
                ) - DebtGuard,
                0
            );

            // Apply guard to debt damage
            var debtAfterDamage = ActionPointDebt + totalDebtDamage;

            // Track if we even have AP left to drain
            var hadNoAPLeftToDrain = ActionPoints == 0;

            // Track any positioning weight to any debt damage we may take
            // NB: Default value is 1 here because that's the max chance. getting hit
            // is the default reaction, not evading
            var positioningHitChance = (float)message.GetValueOrDefault(Consts.POSITIONING_HIT_CHANCE, 1f);

            // If it's external damage, notify with a signal
            if (source != Entity)
            {
                EmitSignal(nameof(TookExternalDamage), source, totalDebtDamage);
            }

            // Even if we received 0 AP drain (perhaps due to a status / ability) we
            // still want to give feedback / SFX
            if (receivedApDrain)
            {
                ActionPoints = Mathf.Min(apAfterDrain, 0);
                // But only if we weren't dry on AP to drain
                if (!hadNoAPLeftToDrain) EmitSignal(nameof(TookApDrain), totalDrain);
            }

            // Handle evasion
            // FIXME: Replace this with positioning
            if (EvadedDamage(source, positioningHitChance))
            {
                var facingDir = source
                    // Get the source facing dir
                    .GetFacingDirTowards(Entity.GlobalPosition)
                    // And reflect it around the Y axis
                    .Reflect(Vector3.Up);
                // Emit with the direction of the evasion
                EmitSignal(nameof(EvadedDebtDamage), facingDir);
            }
            // Otherwise, if we took damage
            else if (receivedDirectApDebt || totalDebtDamage > 0)
            {
                // Cap AP debt at max points
                ActionPointDebt = Mathf.Min(debtAfterDamage, MaxActionPoints);
                if (totalDebtDamage > 0) EmitSignal(nameof(TookDebtDamage), totalDebtDamage);

                // Check for death
                if (ActionPointDebt == MaxActionPoints)
                {
                    EmitSignal(nameof(MaxedOutDebt));
                }
            }
        }

        public virtual void SpendAPOnAction(int amount)
        {
            if (amount > 0)
            {
                // Subtract the given amount by the pawn's current action points and zero check
                ActionPoints = Mathf.Max(ActionPoints - amount, 0);
                EmitSignal(nameof(SpentAp), amount);
            }
        }

        public virtual void RefillApToMax()
        {
            // Calculate the amount we're going to refill to this unit, the
            // difference between their max and the amount of debt
            var availableRefillAmount = MaxActionPoints - ActionPointDebt;

            // Calculate the real amount we refill based on current AP
            var realRefillAmount = availableRefillAmount - ActionPoints;

            // If refill amount is more than 0, refill
            if (realRefillAmount > 0)
            {
                // Set number of action points to the refill amount + max check
                ActionPoints = Mathf.Min(ActionPoints + realRefillAmount, MaxActionPoints);
                EmitSignal(nameof(RecoveredAp), realRefillAmount);
            }
        }

        public virtual void GrantExcessAp(int amount)
        {
            if (amount > 0)
            {
                ActionPointExcess = Mathf.Max(ActionPointExcess + amount, 0);
                EmitSignal(nameof(ReceivedExcessAp), amount);
            }
        }

        public virtual void SpendExcessAp(int amount)
        {
            if (amount > 0)
            {
                // Remove the excess amount and zero check
                ActionPointExcess = Mathf.Max(ActionPointExcess - amount, 0);
                EmitSignal(nameof(SpentExcessAp), amount);
            }
        }

        public virtual void RemoveApDebt(int amount)
        {
            if (amount > 0)
            {
                // Zero out
                ActionPointDebt = Mathf.Max(ActionPointDebt - amount, 0);
                EmitSignal(nameof(HealedDebt), amount);
            }
        }

        // Evasion Check
        private bool EvadedDamage(LevelEntity source, float positioningHitChance)
        {
            // If we're the source, no evasion check
            if (source == Entity) return false;

            // Get our dodge threshold from the positioning weight
            var threshold = Mathf.Max(1f - positioningHitChance, 0f);

            // Get a random float between 1 and 0
            var randResult = _RNG.RandfRange(0.01f, 0.99f);

            // If our threshold is greater than the result, it's a dodge
            return threshold > randResult;
        }
    }
}

