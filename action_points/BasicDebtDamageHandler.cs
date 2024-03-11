using System;
using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.Models;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Positioning;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDebtDamageHandler : DebtDamageHandler
    {
        [Export]
        protected ModelComponent _ModelComponent;

        [Inject]
        private ScreenshakeService _Screenshake;

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
                        var apHandler = payload.ApHandler;
                        // Set debt
                        apHandler.ActionPointDebt = Mathf.Min(
                            payload.DebtAfterDamage,
                            apHandler.MaxActionPoints
                        );

                        // Cap action points
                        apHandler.ActionPoints = Mathf.Min(
                            apHandler.MaxActionPoints - apHandler.ActionPointDebt,
                            apHandler.ActionPoints
                        );

                        // Emit
                        EmitSignal(nameof(TookDebtDamage), payload.TotalDebtDamage);
                    }
                    // If we took no actual debt damage, emit
                    else EmitSignal(nameof(TookNoDamage));
                }),
                // React to damage (or no damage)
                new IfElseCommand(
                    () => payload.TotalDebtDamage > 0,
                    OnTookDamage(payload),
                    OnTookNoDamage(payload)
                )
            );
        }

        protected virtual Command OnTookDamage(ApDamagePayload payload)
        {
            // Take hit animation (only if we took damage)
            return new SeriesCommand(
                // Screenshake
                new ActionCommand(() => _Screenshake.Shake(ScreenshakeHandler.ShakePayload.ShakeSizes.Huge)),
                // React to damage
                _ModelComponent.RunPerformAction(StateConsts.HURT),
                // If still alive and that damage came from a position
                new ConditionalCommand(
                    () => !payload.ApHandler.IsMaxedOut() && payload.Positioning != Positions.Null,
                    new ActionCommand(() =>
                    {
                        // Calculate facing direction of source
                        var targetFacingDir = _ModelComponent.Entity.GetFacingDirTowards(payload.Source.GlobalPosition);
                        _ModelComponent.Entity.FacingDirection = targetFacingDir;
                    })
                )
            );
        }

        protected virtual Command OnTookNoDamage(ApDamagePayload payload)
        {
            return new Command();
        }
    }
}

