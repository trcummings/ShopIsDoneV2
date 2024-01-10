using Godot;
using ShopIsDone.Arenas.ArenaScripts;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Interactables.Interactions
{
    // This is a decorator interaction that wraps the child interaction in an
    // arena script that gets pushed to the script queue so we can have it run
    // after an arena action, and integrated into the arena turn cycle
    [Tool]
	public partial class ArenaScriptInteraction : DecoratorInteraction
	{
        [Inject]
        private ScriptQueueService _ScriptQueue;

        public override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Connect to the interaction
            _DecoratedInteraction.Connect(
                nameof(_DecoratedInteraction.Finished),
                Callable.From(Finish),
                (uint)ConnectFlags.OneShot
            );

            // Turn decorated interaction into an arena script
            var script = new CommandArenaScript()
            {
                CommandFn = () => new InteractionCommand(_DecoratedInteraction)
            };

            // Add the script to the queue
            _ScriptQueue.AddScriptToQueue(script);
        }
    }
}
