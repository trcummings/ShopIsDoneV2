using Godot;
using ShopIsDone.ClownRules.ActionRules;
using ShopIsDone.Utils.Extensions;
using System;
using Godot.Collections;

namespace ShopIsDone.ClownRules.UI
{
    public partial class RulesUI : Control
    {
        [Export]
        public PackedScene RuleScene;

        // Nodes
        private Control _RuleElements;

        public override void _Ready()
        {
            // Ready nodes
            _RuleElements = GetNode<Control>("%Rules");
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
        }
    }
}
