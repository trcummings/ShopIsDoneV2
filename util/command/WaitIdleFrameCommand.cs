using System;
using Godot;

namespace ShopIsDone.Utils.Commands
{
    // This command will wait until the end of the current frame is finished
    // evaluating before finishing. This will not guarantee you a full frame of
    // waiting, unless it is called twice in a row
    public partial class WaitIdleFrameCommand : Command
    {
        private Node _Node;

        public WaitIdleFrameCommand(Node node)
        {
            _Node = node;
        }

        public override void Execute()
        {
            _Node.GetTree().Connect(
                "process_frame",
                new Callable(this, nameof(Finish)),
                (uint)ConnectFlags.OneShot
            );
        }
    }
}