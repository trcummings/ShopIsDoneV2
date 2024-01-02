using System;
using ShopIsDone.ActionPoints;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Positioning;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;
using ApConsts = ShopIsDone.ActionPoints.Consts;

namespace ShopIsDone.Microgames.Outcomes
{
    public partial class EmployeeOutcomeHandler : NodeComponent, IOutcomeHandler
    {
        [Export]
        private ActionPointHandler _ActionPointHandler;

        public Command HandleOutcome(
            Microgame.Outcomes outcome,
            IOutcomeHandler[] _,
            IOutcomeHandler source,
            Positions position = Positions.Null
        )
        {
            var damage = source.GetDamage();

            return new ConditionalCommand(
                // Are we still active check
                Entity.IsActive,
                // Deal damage to ourselves
                _ActionPointHandler.TakeAPDamage(new Dictionary<string, Variant>()
                {
                    { ApConsts.DAMAGE_SOURCE, source.Entity },
                    // If it's a win (for the player) pass along no damage
                    { ApConsts.DEBT_DAMAGE, outcome == Microgame.Outcomes.Win ? 0 : damage.Damage }
                })
            );
        }

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = _ActionPointHandler.ActionPoints,
                Defense = _ActionPointHandler.DebtGuard,
                DrainDefense = _ActionPointHandler.DrainGuard,
                Damage = 1,
                Drain = 0,
                Piercing = 0,
            };
        }
    }
}

