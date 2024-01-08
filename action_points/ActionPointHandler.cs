using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.ActionPoints
{
    public partial class ApDamagePayload : GodotObject
    {
        public ActionPointHandler ApHandler;
        public LevelEntity Source;
        public Positions Positioning;

        // Drain
        public bool HadApLeftToDrain;
        public int TotalDrain;
        public int ApAfterDrain;

        // Debt
        public bool IsDamageDirect;
        public int TotalDebtDamage;
        public int DebtAfterDamage;
    }

    // This is a component that handles damage, debt, guard, and death
    public partial class ActionPointHandler : NodeComponent
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
        public delegate void MaxedOutDebtEventHandler();

        [Export]
        private NodePath _EvasionHandlerPath;
        private IEvasionHandler _EvasionHandler;

        [Export]
        private NodePath _DrainHandlerPath;
        private IDrainHandler _DrainHandler;

        [Export]
        private NodePath _DebtDamageHandlerPath;
        private IDebtDamageHandler _DebtDamageHandler;

        [Export]
        private NodePath _DeathHandlerPath;
        private IDeathHandler _DeathHandler;

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

        public override void _Ready()
        {
            _EvasionHandler = GetNode<IEvasionHandler>(_EvasionHandlerPath);
            _DrainHandler = GetNode<IDrainHandler>(_DrainHandlerPath);
            _DebtDamageHandler = GetNode<IDebtDamageHandler>(_DebtDamageHandlerPath);
            _DeathHandler = GetNode<IDeathHandler>(_DeathHandlerPath);
        }

        // Public API
        public virtual bool HasEnoughAPForAction(int apCost, int excessCost)
        {
            return ActionPoints >= apCost && ActionPointExcess >= excessCost;
        }

        public bool IsMaxedOut()
        {
            return ActionPointDebt >= MaxActionPoints;
        }

        public Command TakeAPDamage(Dictionary<string, Variant> message = null)
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

            // Pull out positioning
            var positioning = (Positions)(int)message.GetValueOrDefault(Consts.POSITIONING, (int)Positions.Null);

            // Create payload
            var payload = new ApDamagePayload()
            {
                ApHandler = this,
                Source = source,
                Positioning = positioning,

                HadApLeftToDrain = !hadNoAPLeftToDrain,
                ApAfterDrain = apAfterDrain,
                TotalDrain = totalDrain,

                IsDamageDirect = receivedDirectApDebt,
                TotalDebtDamage = totalDebtDamage,
                DebtAfterDamage = debtAfterDamage
            };

            return new SeriesCommand(
                // AP DRAIN
                new ConditionalCommand(
                    // Did we actually receive any drain?
                    () => receivedApDrain,
                    _DrainHandler.HandleDrain(payload)
                ),

                // DEBT DAMAGE
                new IfElseCommand(
                    // Evasion check
                    () => _EvasionHandler.EvadedDamage(payload),
                    _EvasionHandler.HandleEvasion(payload),
                    // Otherwise, handle debt damage
                    new SeriesCommand(
                        // Handle damage
                        _DebtDamageHandler.HandleDebtDamage(payload),
                        // Handle death (deferred so we can decide after
                        // damage if we should die
                        new DeferredCommand(() => new ConditionalCommand(
                            () => ActionPointDebt == MaxActionPoints,
                            // If we've maxed out AP debt, run death
                            new DeferredCommand(_DeathHandler.Die)
                        ))
                    )
                )
            );
        }

        public virtual void SpendAPOnAction(int amount)
        {
            if (amount > 0)
            {
                // Subtract the given amount by the pawn's current action points and zero check
                ActionPoints = Mathf.Max(ActionPoints - amount, 0);
                EmitSignal(nameof(SpentAp), -amount);
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
                EmitSignal(nameof(SpentExcessAp), -amount);
            }
        }

        public virtual void RemoveApDebt(int amount)
        {
            if (amount > 0)
            {
                // Zero out
                ActionPointDebt = Mathf.Max(ActionPointDebt - amount, 0);
                EmitSignal(nameof(HealedDebt), -amount);
            }
        }
    }
}

