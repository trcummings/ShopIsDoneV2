using System;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
	public partial class EmployeeDebtDamageHandler : BasicDebtDamageHandler
	{
		[Export]
		private TaskHandler _TaskHandler;

        public override Command HandleDebtDamage(ActionPointHandler apHandler, LevelEntity source, int totalDebtDamage, int debtAfterDamage)
        {
			return new SeriesCommand(
				// Interrupt current task check
				new ConditionalCommand(
					// If the damage isn't self-inflicted, and also not from our
					// current task, interrupt the current task
					() =>
					source != apHandler.Entity &&
					_TaskHandler.HasCurrentTask() &&
					source != _TaskHandler.CurrentTask?.Entity,
					// Run deferred
					new DeferredCommand(_TaskHandler.StopDoingTask)
				),
                // Inflict damage
                InflictDamage(apHandler, totalDebtDamage, debtAfterDamage)
            );
        }
    }
}

