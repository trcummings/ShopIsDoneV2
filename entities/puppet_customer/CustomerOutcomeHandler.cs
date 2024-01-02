using System;
using ShopIsDone.ActionPoints;
using Godot;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils.Positioning;
using ShopIsDone.Utils.Commands;
using ApConsts = ShopIsDone.ActionPoints.Consts;
using ShopIsDone.Microgames;
using ShopIsDone.Core;
using ShopIsDone.Microgames.Outcomes;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class CustomerOutcomeHandler : NodeComponent, IOutcomeHandler
    {
        [Export]
        private ActionPointHandler _ActionPointHandler;

        public Command HandleOutcome(
            Microgame.Outcomes outcome,
            IOutcomeHandler[] targets,
            IOutcomeHandler _,
            Positions position = Positions.Null
        )
        {
            return new SeriesCommand(
                targets.Select(t => {
                    var damage = t.GetDamage();
                    var employee = t.Entity;

                    return new ConditionalCommand(
                        // Are we still active check
                        Entity.IsActive,
                        // Deal damage to us
                        _ActionPointHandler.TakeAPDamage(new Dictionary<string, Variant>()
                        {
                            { ApConsts.DAMAGE_SOURCE, employee },
                            // If the outcome is a win (for the player),
                            // pass along full damage
                            { ApConsts.DEBT_DAMAGE, outcome == Microgame.Outcomes.Win ? damage.Damage : 0 }
                        })
                    );
                }).ToArray()
            );
        }

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = _ActionPointHandler.ActionPoints,
                Defense = 0,
                DrainDefense = 0,
                Damage = 1,
                Drain = 1,
                Piercing = 0
            };
        }
    }
}

