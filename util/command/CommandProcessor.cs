using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Utils.Commands
{
    public interface IProcessableCommand
    {
        void Process(double delta);
    }

    public interface IPhysicsProcessableCommand
    {
        void PhysicsProcess(double delta);
    }

    public partial class CommandProcessor : Node
    {
        protected List<IProcessableCommand> ProcessableCommands = new List<IProcessableCommand>();
        protected List<IPhysicsProcessableCommand> PhysicsProcessableCommands = new List<IPhysicsProcessableCommand>();

        public void AddCommand(Command command)
        {
            // Add commands if they're processable
            if (command is IProcessableCommand processable)
            {
                ProcessableCommands.Add(processable);
            }
            if (command is IPhysicsProcessableCommand physicsProcessable)
            {
                PhysicsProcessableCommands.Add(physicsProcessable);
            }
        }

        public void RemoveCommand(Command command)
        {
            // Remove commands from their processable lists
            if (command is IProcessableCommand processable)
            {
                ProcessableCommands.Remove(processable);
            }
            if (command is IPhysicsProcessableCommand physicsProcessable)
            {
                PhysicsProcessableCommands.Remove(physicsProcessable);
            }
        }

        public override void _Process(double delta)
        {
            // NB: Duplicate list to not cause mutations
            foreach (var processable in ProcessableCommands.ToList())
            {
                processable.Process(delta);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            // NB: Duplicate list to not cause mutations
            foreach (var processable in PhysicsProcessableCommands.ToList())
            {
                processable.PhysicsProcess(delta);
            }
        }
    }
}