using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Arenas.ArenaScripts
{
    // This is kind of like a subclass sandbox for interacting with the world
    public partial class ArenaScript : Resource
    {
        public virtual void Init()
        {

        }

        public virtual Command ExecuteScript()
        {
            return new Command();
        }
    }

    public partial class CommandArenaScript : ArenaScript
    {
        public Func<Command> CommandFn;

        public override Command ExecuteScript()
        {
            return CommandFn.Invoke();
        }
    }
}