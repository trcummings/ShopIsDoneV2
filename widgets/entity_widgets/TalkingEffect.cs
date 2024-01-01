using Godot;
using System;

namespace ShopIsDone.Widgets
{
	public partial class TalkingEffect : Node3D
	{
        [Signal]
        public delegate void FinishedEventHandler();

        // Nodes
        private AnimationPlayer _AnimationPlayer;
        private MeshInstance3D _Mesh;

        public override void _Ready()
        {
            // Ready nodes
            _AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
            _Mesh = GetNode<MeshInstance3D>("%MeshInstance");
        }

        public void Popup(bool small = false)
        {
            _Mesh.Scale = small? new Vector3(0.5f, 1, 0.5f) : Vector3.One;
            _AnimationPlayer.Connect(
                "animation_finished",
                new Callable(this, nameof(OnAnimationFinished)),
                (uint)ConnectFlags.OneShot
            );
            _AnimationPlayer.Play("default");
        }

        private void OnAnimationFinished(string _)
        {
            EmitSignal(nameof(Finished));
        }
    }
}
