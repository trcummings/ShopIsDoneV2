using System;
using Godot;
using ShopIsDone.Cameras;
using ShopIsDone.EntityStates;
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
        protected EntityStateHandler _StateHandler;

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
                _StateHandler.RunChangeState(StateConsts.HURT),
                // If still alive
                new ConditionalCommand(
                    () => !payload.ApHandler.IsMaxedOut(),
                    // Go back to idle
                    new SeriesCommand(
                        _StateHandler.RunChangeState(StateConsts.IDLE),
                        // If that damage came from a position, face that
                        // position
                        new ConditionalCommand(
                            () => payload.Positioning != Positions.Null,
                            new ActionCommand(() =>
                            {
                                // Calculate facing direction of source
                                var targetFacingDir = _StateHandler.Entity.GetFacingDirTowards(payload.Source.GlobalPosition);
                                _StateHandler.Entity.FacingDirection = targetFacingDir;
                            })
                        )
                    )
                )
            );
        }

        protected virtual Command OnTookNoDamage(ApDamagePayload payload)
        {
            return _StateHandler.RunChangeState(StateConsts.IDLE);
        }
    }
}

