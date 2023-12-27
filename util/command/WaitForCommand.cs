using System;
using Godot;

namespace ShopIsDone.Utils.Commands
{
    public partial class WaitForCommand : Command
    {
        private Node _Node;
        private float _Timeout;

        public WaitForCommand(Node node, float timeout)
        {
            _Node = node;
            _Timeout = timeout;
        }

        public override void Execute()
        {
            // One shot connection to timer timeout
            _Node
                .GetTree()
                .CreateTimer(_Timeout)
                .Connect("timeout", new Callable(this, nameof(Finish)), (uint)ConnectFlags.OneShot);
        }
    }
}