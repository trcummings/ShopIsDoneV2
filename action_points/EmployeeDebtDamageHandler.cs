using System;
using Godot;
using ShopIsDone.Tasks;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
	public partial class EmployeeDebtDamageHandler : BasicDebtDamageHandler
	{
		[Export]
		private TaskHandler _TaskHandler;

        public override Command HandleDebtDamage(ApDamagePayload payload)
        {
			return new SeriesCommand(
				// Interrupt current task check
				new ConditionalCommand(
					// If the damage isn't self-inflicted, and also not from our
					// current task, interrupt the current task
					() =>
                    payload.Source != payload.ApHandler.Entity &&
					_TaskHandler.HasCurrentTask() &&
					payload.Source != _TaskHandler.CurrentTask?.Entity,
					// Run deferred
					new DeferredCommand(_TaskHandler.StopDoingTask)
				),
                // Inflict damage
                InflictDamage(payload)
            );
        }
    }
}

