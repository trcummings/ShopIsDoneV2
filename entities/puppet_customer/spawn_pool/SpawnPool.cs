using Godot;
using System;

namespace ShopIsDone.Entities.PuppetCustomers
{
    public partial class SpawnPool : MeshInstance3D
    {
        [Signal]
        public delegate void AppearedEventHandler();

        [Signal]
        public delegate void DisappearedEventHandler();

        // Nodes
        [Export]
        private AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            // Initially hide
            Hide();
            _AnimPlayer.Connect("animation_finished", new Callable(this, nameof(OnAnimFinished)));
        }

        public void Appear()
        {
            _AnimPlayer.Play("Appear");
            Show();
        }

        public void Disappear()
        {
            _AnimPlayer.Play("Disappear");
            Hide();
        }

        private void OnAnimFinished(string animName)
        {
            if (animName == "Appear") EmitSignal(nameof(Appeared));
            else if (animName == "Disappear") EmitSignal(nameof(Disappeared));
        }
    }
}
