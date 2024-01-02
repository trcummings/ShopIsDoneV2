using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.ActionPoints
{
    public interface IDrainHandler
    {
        Command HandleDrain(ActionPointHandler apHandler, bool receivedDrain, bool hadApLeftToDrain, int apAfterDrain);
    }

    public partial class DrainHandler : Node, IDrainHandler
    {
        [Signal]
        public delegate void TookApDrainEventHandler(int amount);

        public virtual Command HandleDrain(ActionPointHandler apHandler, bool receivedDrain, bool hadApLeftToDrain, int apAfterDrain)
        {
            return new Command();
        }
    }
}

