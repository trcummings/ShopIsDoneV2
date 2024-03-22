using Godot;
using System;

namespace ShopIsDone.FX
{
    // Simple class wrapper for shader effect that we want to trigger globally
    public partial class WipeEffect : SubViewportContainer
    {
        private ShaderMaterial _Material;

        public override void _Ready()
        {
            base._Ready();

            _Material = (ShaderMaterial)Material;
            // Connect to global trigger
            var events = Events.GetEvents(this);
            events.BloodWipeRequested += RunWipeEffect;
        }

        public void RunWipeEffect()
        {
            var tween = CreateTween().BindNode(this).SetEase(Tween.EaseType.OutIn);
            tween.TweenMethod(new Callable(this, nameof(SetTime)), 0f, 2f, 0.75f);
        }

        private void SetTime(float time)
        {
            _Material.SetShaderParameter("time", time);
        }
    }
}

