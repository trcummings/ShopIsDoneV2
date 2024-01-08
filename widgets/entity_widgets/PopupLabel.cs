using Godot;
using System;

namespace ShopIsDone.Widgets
{
    public partial class PopupLabel : Node3D
    {
        [Signal]
        public delegate void FinishedEventHandler();

        // Nodes
        private Label3D _Label;
        private AnimationPlayer _AnimationPlayer;

        public override void _Ready()
        {
            // Ready nodes
            _Label = GetNode<Label3D>("%Label");
            _AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

            // Hide the damage label initially
            _Label.Hide();
        }

        public void PopupNumber(int amount, Color textColor, Color outlineColor)
        {
            if (amount == 0) PopupText("0", textColor, outlineColor);
            else PopupText(amount > 0 ? $"+{amount}" : $"{amount}", textColor, outlineColor);
        }

        public void PopupText(string text, Color textColor, Color outlineColor)
        {
            // Set the damage label properties
            _Label.Text = text;
            _Label.Modulate = textColor;
            _Label.OutlineModulate = outlineColor;

            // Show the label
            _Label.Show();

            // Arc that number
            _AnimationPlayer.Connect(
                "animation_finished",
                new Callable(this, nameof(OnAnimationFinished)),
                (uint)ConnectFlags.OneShot
            );
            _AnimationPlayer.Play("popup");
        }

        private void OnAnimationFinished(string _)
        {
            // Hide it
            _Label.Hide();

            EmitSignal(nameof(Finished));
        }
    }
}

