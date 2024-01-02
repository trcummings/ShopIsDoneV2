using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDebtDamageHandler : DebtDamageHandler
	{
        [Export]
        protected EntityStateHandler _StateHandler;

        public override Command HandleDebtDamage(ActionPointHandler apHandler, LevelEntity source, int totalDebtDamage, int debtAfterDamage)
        {
            return InflictDamage(apHandler, totalDebtDamage, debtAfterDamage);
        }

        protected Command InflictDamage(ActionPointHandler apHandler, int totalDebtDamage, int debtAfterDamage)
        {
            return new SeriesCommand(
                // Inflict damage
                new ActionCommand(() =>
                {
                    // Set debt
                    if (totalDebtDamage > 0)
                    {
                        EmitSignal(nameof(TookDebtDamage), totalDebtDamage);
                        apHandler.ActionPointDebt = Mathf.Min(debtAfterDamage, apHandler.MaxActionPoints);
                    }
                    // If we took no actual debt damage, emit
                    else EmitSignal(nameof(TookNoDamage));
                }),
                // Take hit, then return to idle
                _StateHandler.RunChangeState("hurt"),
                _StateHandler.RunChangeState("idle")
            );
        }
    }
}

