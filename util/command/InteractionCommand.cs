using System;
using Godot;
using ShopIsDone.Interactables.Interactions;

namespace ShopIsDone.Utils.Commands
{
    public partial class InteractionCommand : Command
    {
        private Interaction _Interaction;

        public InteractionCommand(Interaction interaction)
        {
            _Interaction = interaction;
        }

        public override void Execute()
        {
            _Interaction.Connect(
                nameof(_Interaction.Finished),
                Callable.From(Finish),
                (uint)ConnectFlags.OneShot
            );
            _Interaction.Execute();
        }
    }
}

