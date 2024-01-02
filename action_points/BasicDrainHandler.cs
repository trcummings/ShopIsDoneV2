using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public partial class BasicDrainHandler : DrainHandler
	{
        public override Command HandleDrain(ApDamagePayload payload)
        {
            return new SeriesCommand(
                new ActionCommand(() =>
                {
                    // Apply drain
                    payload.ApHandler.ActionPoints = Mathf.Min(payload.ApAfterDrain, 0);
                    // Emit
                    if (payload.HadApLeftToDrain) EmitSignal(nameof(TookApDrain), payload.TotalDrain);
                }),
                // Just wait a moment for the effects to complete because we don't
                // have a dedicated state transition for drain
                new WaitForCommand(this, 0.35f)
            );
        }
    }
}

