using Godot;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Widgets
{
	public partial class AlertEffect : Node3D
	{
        [Signal]
        public delegate void FinishedEventHandler();

        // Nodes
        private AnimationPlayer _AnimationPlayer;

        public override void _Ready()
        {
            // Ready nodes
            _AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
        }

        public void Popup()
        {
            // Arc that number
            _AnimationPlayer.Connect(
                "animation_finished",
                new Callable(this, nameof(OnAnimationFinished)),
                (uint)ConnectFlags.OneShot
            );
            _AnimationPlayer.Play("default");
        }

        public async Task PopupAsync()
        {
            Popup();
            await ToSignal(this, nameof(Finished));
        }

        private void OnAnimationFinished(string _)
        {
            EmitSignal(nameof(Finished));
        }
    }
}