using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDrainHandler : DrainHandler
	{
        public override Command HandleDrain(ActionPointHandler apHandler, bool hadApLeftToDrain, int totalDrain, int apAfterDrain)
        {
            return new SeriesCommand(
                new ActionCommand(() =>
                {
                    // Apply drain
                    apHandler.ActionPoints = Mathf.Min(apAfterDrain, 0);
                    // Emit
                    if (hadApLeftToDrain) EmitSignal(nameof(TookApDrain), totalDrain);
                }),
                // Just wait a moment for the effects to complete because we don't
                // have a dedicated state transition for drain
                new WaitForCommand(this, 0.35f)
            );
        }
    }
}

