using Godot;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.UI
{
    public partial class MoreInfoUIContainer : Control
    {
        [Export]
        public PackedScene MoreInfoPanel;

        // Nodes
        private TabContainer _InfoTabs;

        public override void _Ready()
        {
            // Ready nodes
            _InfoTabs = GetNode<TabContainer>("%InfoTabs");
        }

        public void Init(MoreInfoPayload payload)
        {
            var node = MoreInfoPanel.Instantiate<MoreInfoPanel>();
            _InfoTabs.AddChild(node);
            node.Init(payload);

            // Select the first tab
            if (_InfoTabs.GetTabCount() > 0) _InfoTabs.CurrentTab = 0;
        }

        public void Reset()
        {
            // Remove all tabs
            _InfoTabs.RemoveChildrenOfType<MoreInfoPanel>();
        }

        public override void _Process(double delta)
        {
            // Listen for tab change input
        }
    }
}
