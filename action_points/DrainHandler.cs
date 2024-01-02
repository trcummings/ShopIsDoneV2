using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IDrainHandler
    {
        Command HandleDrain(ApDamagePayload payload);
    }

    public partial class DrainHandler : Node, IDrainHandler
    {
        [Signal]
        public delegate void TookApDrainEventHandler(int amount);

        public virtual Command HandleDrain(ApDamagePayload payload)
        {
            return new Command();
        }
    }
}

