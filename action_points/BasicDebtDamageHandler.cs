using System;
using Godot;
using ShopIsDone.EntityStates;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Positioning;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDebtDamageHandler : DebtDamageHandler
	{
        [Export]
        protected EntityStateHandler _StateHandler;

        public override Command HandleDebtDamage(ApDamagePayload payload)
        {
            return InflictDamage(payload);
        }

        protected Command InflictDamage(ApDamagePayload payload)
        {
            return new SeriesCommand(
                // Inflict damage
                new ActionCommand(() =>
                {
                    // Set debt
                    if (payload.TotalDebtDamage > 0)
                    {
                        // Set debt
                        payload.ApHandler.ActionPointDebt = Mathf.Min(
                            payload.DebtAfterDamage,
                            payload.ApHandler.MaxActionPoints
                        );

                        // Emit
                        EmitSignal(nameof(TookDebtDamage), payload.TotalDebtDamage);
                    }
                    // If we took no actual debt damage, emit
                    else EmitSignal(nameof(TookNoDamage));
                }),
                // Take hit animation (only if we took damage)
                new ConditionalCommand(
                    () => payload.TotalDebtDamage > 0,
                    _StateHandler.RunChangeState("hurt")
                ),
                // Set facing direction
                new ConditionalCommand(
                    () => payload.Positioning != Positions.Null,
                    new ActionCommand(() =>
                    {
                        // Calculate facing direction of source
                        var targetFacingDir = _StateHandler.Entity.GetFacingDirTowards(payload.Source.GlobalPosition);
                        _StateHandler.Entity.FacingDirection = targetFacingDir;
                    })
                ),
                // Return to idle
                _StateHandler.RunChangeState("idle")
            );
        }
    }
}

