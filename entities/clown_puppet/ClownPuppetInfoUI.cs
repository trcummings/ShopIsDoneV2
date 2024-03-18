using Godot;
using ShopIsDone.Arenas.UI;
using ShopIsDone.Core;
using System;

namespace ShopIsDone.Entities.ClownPuppet
{
    public partial class ClownPuppetInfoUI : TargetInfoUI
    {
        private TextureRect _ClownFeed;
        private bool _IsAvailable = true;

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

            var viewport = entity.GetNode<SubViewport>("FaceCamViewport");
            _ClownFeed.Texture = viewport.GetTexture();
        }

        public override void RequestInfoPayload(Action<MoreInfoPayload> _)
        {
            // Emit more info warning effect
            GD.Print("No no no. No.");

            // Set availability flag
            _IsAvailable = false;

            // Set three second timer to disable UI for
            GetTree().CreateTimer(3).Connect(
                "timeout",
                Callable.From(() => _IsAvailable = true),
                (uint)ConnectFlags.OneShot
            );
        }
    }
}

