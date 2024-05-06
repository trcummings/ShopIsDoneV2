using Godot;
using System;

namespace ShopIsDone.Conditions.UI
{
    public partial class ConditionUIElement : Control
    {
        // State
        private Condition _Condition;

        // Nodes
        private CheckBox _CheckBox;
        private Label _Label;

        public override void _Ready()
        {
            // Ready nodes
            _CheckBox = GetNode<CheckBox>("%CheckBox");
            _Label = GetNode<Label>("%Label");
        }

        public bool HasCondition(Condition condition)
        {
            return _Condition.HasCondition(condition);
        }

        public void SetCondition(Condition condition)
        {
            // Set the new condition
            _Condition = condition;

            // Update the UI
            UpdateCondition();
        }

        public void UpdateCondition()
        {
            // Set the checkbox
            _CheckBox.ButtonPressed = _Condition.IsComplete();

            // Set the label
            _Label.Text = _Condition.Description;
        }
    }
}
