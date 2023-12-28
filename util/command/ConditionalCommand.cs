using System;
using Godot;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Utils.Commands
{
    // This decorator runs a command only if the given conditional function resolves
    // to true
    public partial class ConditionalCommand : Command
    {
        public ConditionalCommand(Func<bool> conditional, Command command)
        {
            Command = command;
            Conditional = conditional;
        }

        // The decorated command in question
        protected Command Command;

        // This is a conditional that's meant to 
        protected Func<bool> Conditional;

        public override void Execute()
        {
            // If true, run the command
            if (Conditional())
            {
                // Oneshot connect to finished signal
                Command.Connect(
                    nameof(Command.Finished),
                    new Callable(this, nameof(Finish)),
                    (uint)ConnectFlags.OneShot
                );
                // Run command
                Command.Execute();
            }
            // Otherwise, finish normally
            else Finish();
        }
    }
}
