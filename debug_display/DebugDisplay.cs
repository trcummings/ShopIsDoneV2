using Godot;
using ShopIsDone.Game;

namespace ShopIsDone.Debug
{
    public partial class DebugDisplay : CanvasLayer
    {
        private Control _PanelContainer;

        public override void _Ready()
        {
            // Ready nodes
            _PanelContainer = GetNode<Control>("%PanelContainer");

            // Initially hide
            SetVisibility(false);

            // Connect to Events
            var events = Events.GetEvents(this);
            events.AddDebugPanelRequested += (Control panel) => _PanelContainer.AddChild(panel);
            events.RemoveDebugPanelRequested += _PanelContainer.RemoveChild;
        }

        public void SetVisibility(bool value)
        {
            // If this is a debug build, set visibility manually
            if (GameManager.IsDebugMode() && value) Show();
            else Hide();
        }
    }
}