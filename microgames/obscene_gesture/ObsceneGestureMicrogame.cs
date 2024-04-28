using System;
using Godot;

namespace ShopIsDone.Microgames.ObsceneGesture
{
    public partial class ObsceneGestureMicrogame : Microgame
	{
        [Export]
        private AnimationPlayer _AnimPlayer;

        public override void Init(MicrogamePayload payload)
        {
            base.Init(payload);
            HideTimer(0.001f);
        }

        public override void Start()
        {
            // Oneshot connect to the animation player
            _AnimPlayer.Connect(
                "animation_finished",
                Callable.From((string _) =>
                {
                    EmitSignal(nameof(MicrogameFinished), (int)Outcomes.Loss);
                }),
                (uint)ConnectFlags.OneShot
            );
            _AnimPlayer.Play("default");
        }
    }
}

