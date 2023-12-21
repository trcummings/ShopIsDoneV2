using System;
using System.Linq;
using Godot;

namespace ShopIsDone.Utils.Commands
{
    // This decorator takes in a function that produces a command as an argument,
    // and defers the execution of the function until the command itself is executed
    public partial class DeferredCommand : Command
    {
        protected Func<Command> DeferredCommandFunc;

        // Undo vars
        private Command _DeferredCommand;

        // Constructor
        public DeferredCommand(Func<Command> deferredCommandFunc)
        {
            DeferredCommandFunc = deferredCommandFunc;
        }

        public override void Execute()
        {
            // Evaluate the deferred command
            _DeferredCommand = DeferredCommandFunc.Invoke();

            // Execute the deferred command
            _DeferredCommand.Finished += OnDeferredCommandFinished;
            _DeferredCommand.Execute();
        }

        private void OnDeferredCommandFinished()
        {
            _DeferredCommand.Finished -= OnDeferredCommandFinished;
            Finish();
        }

        public static Command DeferredSeries(params Command[] commands)
        {
            // Escape case: we should never get here, but if we do, fail gracefully
            if (commands.Length == 0) return new Command();
            // Base case (just return the command)
            if (commands.Length == 1) return commands[0];
            // Induction step
            return new DeferredCommand(() => new SeriesCommand(
                commands.First(),
                DeferredSeries(commands.Skip(1).ToArray())
            ));
        }
    }


}

