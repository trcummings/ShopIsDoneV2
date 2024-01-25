using Godot;

namespace ShopIsDone.Interactables.Interactions
{
    [Tool]
	public partial class ChangeLevelInteraction : Interaction
	{
        [Export]
        private PackedScene _ToLevel;

        public override void Execute()
        {
            var events = Events.GetEvents(this);
            events.EmitSignal(nameof(events.LevelChangeRequested), _ToLevel);

            Finish();
        }
    }
}
