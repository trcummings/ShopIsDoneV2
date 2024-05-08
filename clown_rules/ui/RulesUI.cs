using Godot;
using ShopIsDone.ClownRules.ActionRules;
using ShopIsDone.Utils.Extensions;
using System;
using Godot.Collections;
using ShopIsDone.UI;

namespace ShopIsDone.ClownRules.UI
{
    public partial class RulesUI : Control, IVerticalDrawerChild
    {
        [Export]
        public PackedScene RuleScene;

        // Nodes
        private Control _RuleElements;
        private Control _TitleContainer;
        public Control DrawerFace { get { return _TitleContainer; } }

        public override void _Ready()
        {
            // Ready nodes
            _RuleElements = GetNode<Control>("%DrawerContents");
            _TitleContainer = GetNode<Control>("%TitleContainer");
        }

        public void Init(Array<ClownActionRule> rules)
        {
            // Clear away any existing rules
            _RuleElements.RemoveChildrenOfType<RuleUIElement>();

            // Create a list of condition elements
            foreach (var rule in rules)
            {
                var elem = RuleScene.Instantiate<RuleUIElement>();
                _RuleElements.AddChild(elem);
                elem.Text = rule.RuleDescription;
            }

            if (rules.Count == 0)
            {
                (GetParent() as Control).Hide();
                Hide();
            }
            else
            {
                (GetParent() as Control).Show();
                Show();
            }
        }
    }
}
