using System;
using Godot;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Utils.Commands
{
    public partial class ParallelCommand : Command
    {
        private Array<Command> _Commands = new Array<Command>();
        private int _FinishedCommands = 0;

        public ParallelCommand(params Command[] commands)
        {
            _Commands = commands.ToGodotArray();
        }

        public override void Execute()
        {
            // If no commands, just emit and end
            if (_Commands.Count == 0)
            {
                CallDeferred(nameof(Finish));
                return;
            }

            // Otherwise, connect to each command first
            foreach (var command in _Commands)
            {
                command.Connect(
                    nameof(Finished),
                    new Callable(this, nameof(OnCommandFinished)),
                    (uint)ConnectFlags.OneShot
                );
            }

            // Then run them all
            foreach (var command in _Commands) command.Execute();
        }

        private void OnCommandFinished()
        {
            _FinishedCommands += 1;
            // If all commands are finished, finish this one
            if (_FinishedCommands == _Commands.Count) Finish();
        }
    }
}