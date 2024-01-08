using System;
using Godot;

namespace ShopIsDone.Utils.Commands
{
    // This decorator acts as command control flow for two different commands
    public partial class IfElseCommand : Command
    {
        public IfElseCommand(Func<bool> conditional, Command ifCommand, Command elseCommand)
        {
            IfCommand = ifCommand;
            ElseCommand = elseCommand;
            Conditional = conditional;
        }

        // The decorated commands in question
        protected Command IfCommand;
        protected Command ElseCommand;

        // This is a conditional that's meant to decide between the two paths
        protected Func<bool> Conditional;

        // NB: This is just for reference tracking
        public Command _ChosenCommand = null;

        public override void Execute()
        {
            // If true, run the IF command
            if (Conditional()) _ChosenCommand = IfCommand;
            // Otherwise, run the ELSE command
            else _ChosenCommand = ElseCommand;

            _ChosenCommand.Finished += OnSubCommandFinished;

            // HACK: This runs some kind of effect that forces the connections to stay
            // in memory so we have no problem chaining async commands together
            var _ = _ChosenCommand.GetSignalConnectionList(nameof(Finished));

            _ChosenCommand.Execute();
        }

        private void OnSubCommandFinished()
        {
            // Disconnect from command
            _ChosenCommand.Finished -= OnSubCommandFinished;

            // Finish command
            Finish();

            // Null out chosen command
            _ChosenCommand = null;
        }
    }
}