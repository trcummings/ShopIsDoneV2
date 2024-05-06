using Godot;
using System;

namespace ShopIsDone.FX
{
    // Simple class wrapper for shader effect that we want to trigger globally
    public partial class WipeEffect : SubViewportContainer
    {
        private ShaderMaterial _Material;
        private Callable _SetTime;

        public override void _Ready()
        {
            base._Ready();

            _Material = (ShaderMaterial)Material;
            _SetTime = new Callable(this, nameof(SetTime));

            // Connect to global trigger
            var events = Events.GetEvents(this);
            events.Connect(nameof(events.BloodWipeRequested), Callable.From(RunWipeEffect));
        }

        public void RunWipeEffect()
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .SetEase(Tween.EaseType.OutIn)
                .TweenMethod(_SetTime, 0f, 2f, 0.75f);
        }

        private void SetTime(float time)
        {
            _Material.SetShaderParameter("time", time);
        }
    }
}

