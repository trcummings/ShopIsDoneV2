using System;
using Godot;
using ShopIsDone.Levels;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
	public partial class DoorInteraction : Interaction
	{
        [Export]
        private DirectionalPoint _WarpPoint;

        [Inject]
        private PlayerCharacterManager _PlayerCharacterManager;

        public async override void Execute()
        {
            // Inject
            InjectionProvider.Inject(this);

            // Get the global events
            var events = Events.GetEvents(this);

            // Request a fade in
            events.EmitSignal(nameof(events.FadeInRequested));
            await ToSignal(events, nameof(events.FadeInFinished));

            // Warp the units to the warp point
            _PlayerCharacterManager.WarpGroupToPosition(_WarpPoint.GlobalPosition, _WarpPoint.FacingDirection);

            // Request a fade out
            events.EmitSignal(nameof(events.FadeOutRequested));
            await ToSignal(events, nameof(events.FadeOutFinished));
        }
    }
}

