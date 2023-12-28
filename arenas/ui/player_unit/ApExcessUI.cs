using Godot;
using System;

namespace ShopIsDone.Arenas.UI
{
    public partial class ApExcessUI : Control
    {
        // Nodes
        private ActionPointBulb _Bulb;
        private Label _ExcessLabel;
        private TextureRect _DirectionArrow;

        // State
        private int _ApExcess;

        public override void _Ready()
        {
            // Ready nodes
            _Bulb = GetNode<ActionPointBulb>("%ApExcessBulb");
            _ExcessLabel = GetNode<Label>("%ApExcessLabel");
            _DirectionArrow = GetNode<TextureRect>("%DirectionArrow");

            // Set excess bulb state
            _Bulb.SetBulbState(ActionPointBulb.BulbStates.Excess);

            // Hide dir arrow;
            _DirectionArrow.Hide();
        }

        public void SetApExcess(int amount)
        {
            _ApExcess = amount;
            _ExcessLabel.Text = amount.ToString();
        }

        public void SetApDiff(int amount)
        {
            // Clear custom colors
            ClearTextColors();

            // If there's no change, return early
            if (amount == _ApExcess) return;

            // Positive change
            if (amount > _ApExcess)
            {
                _ExcessLabel.AddThemeColorOverride("font_color", Colors.Green);
                _DirectionArrow.Modulate = Colors.Green;
                _DirectionArrow.FlipV = false;
            }
            // Negative change
            else
            {
                _ExcessLabel.AddThemeColorOverride("font_color", Colors.Red);
                _DirectionArrow.Modulate = Colors.Red;
                _DirectionArrow.FlipV = true;
            }

            // Show the arrow
            _DirectionArrow.Show();

            // Set the label
            _ExcessLabel.Text = amount.ToString();
        }

        public void ClearApDiff()
        {
            // Clear custom colors
            ClearTextColors();

            // Hide the arrow
            _DirectionArrow.Hide();

            // Reset the label
            _ExcessLabel.Text = _ApExcess.ToString();
        }

        private void ClearTextColors()
        {
            if (_ExcessLabel.HasThemeColorOverride("font_color"))
                _ExcessLabel.RemoveThemeColorOverride("font_color");
            if (_ExcessLabel.HasThemeColorOverride("font_outline_modulate"))
                _ExcessLabel.RemoveThemeColorOverride("font_outline_modulate");
        }
    }
}