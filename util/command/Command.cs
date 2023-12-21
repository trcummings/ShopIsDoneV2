using System;
using Godot;

namespace ShopIsDone.Utils.Commands
{
    public partial class Command : GodotObject
    {
        [Signal]
        public delegate void FinishedEventHandler();

        public virtual void Execute()
        {
            // Finish MUST be called at the end of every single execution, async code
            // within or no. Here we make null commands call finish as deferred so
            // we don't hang if there's same frame evaluation going on
            CallDeferred("Finish");
        }

        protected void Finish()
        {
            EmitSignal(nameof(Finished));
        }
    }
}

