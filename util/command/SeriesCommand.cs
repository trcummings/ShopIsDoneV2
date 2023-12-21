using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Utils.Commands
{
    public partial class SeriesCommand : Command
    {
        public bool Log = false;

        public SeriesCommand(params Command[] commands)
        {
            Commands = commands.ToList();
        }

        protected List<Command> Commands = new List<Command>();

        // State vars
        public Queue<Command> _CandidateCommands;
        // NB: This is just for reference preservation
        public Command _CurrentCommand = null;

        public override void Execute()
        {
            // If no commands, just emit and end
            if (Commands.Count == 0)
            {
                CallDeferred("Finish");
                return;
            }

            // Set candidate commands from given commands
            _CandidateCommands = new Queue<Command>(Commands);

            // Start command cycle
            RunNextCommand();
        }

        private void OnSubCommandFinished()
        {
            // Disconnect
            if (_CurrentCommand != null)
            {
                _CurrentCommand.Finished -= OnSubCommandFinished;
            }

            // Check length of candidate commands, if empty, emit finished
            if (_CandidateCommands.Count == 0)
            {
                Finish();
                _CurrentCommand = null;
            }
            // Otherwise, start the cycle with the next command
            else RunNextCommand();
        }

        private void RunNextCommand()
        {
            // Pop off first element
            _CurrentCommand = _CandidateCommands.Dequeue();

            // Connect to its finished signal
            _CurrentCommand.Finished += OnSubCommandFinished;

            // HACK: This runs some kind of effect that forces the connections to stay
            // in memory so we have no problem chaining async commands together
            var _ = _CurrentCommand.GetSignalConnectionList(nameof(Finished));

            if (Log)
            {
                GD.Print("Running ", _CurrentCommand.GetType(), " in SeriesCommand");
                GD.Print(_CurrentCommand.GetSignalConnectionList(nameof(Finished)));
            }

            // Run it
            _CurrentCommand.Execute();
        }
    }
}

