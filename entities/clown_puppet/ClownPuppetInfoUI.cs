using Godot;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Cameras;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.Entities.ClownPuppet
{
    public partial class ClownPuppetInfoUI : TargetInfoUI
    {
        [Signal]
        public delegate void ClownEffectEventHandler();

        private TextureRect _ClownFeed;
        private bool _IsAvailable = true;

        [Inject]
        private ScreenshakeService _Screenshake;

        public override void _Ready()
        {
            _ClownFeed = GetNode<TextureRect>("%ClownFeed");
        }

        public override bool IsAvailable()
        {
            return _IsAvailable;
        }

        public override bool IsValidEntityForUI(LevelEntity entity)
        {
            return entity.Id == "clown_puppet";
        }

        public override void Init(LevelEntity entity)
        {
            base.Init(entity);
            InjectionProvider.Inject(this);

            var viewport = entity.GetNode<SubViewport>("FaceCamViewport");
            _ClownFeed.Texture = viewport.GetTexture();
        }

        public override void RequestInfoPayload(Action<MoreInfoPayload> _)
        {
            // Emit more info warning effect
            var events = Events.GetEvents(this);
            events.EmitSignal(nameof(events.BloodWipeRequested));
            EmitSignal(nameof(ClownEffect));

            // Screenshake
            _Screenshake.Shake(ScreenshakeHandler.ShakePayload.ShakeSizes.Huge);

            // Set availability flag
            _IsAvailable = false;

            // Set three second timer to disable UI for
            GetTree().CreateTimer(5).Connect(
                "timeout",
                Callable.From(() => _IsAvailable = true),
                (uint)ConnectFlags.OneShot
            );
        }
    }
}

