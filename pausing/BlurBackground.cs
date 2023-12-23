using Godot;
using System;

namespace ShopIsDone.Pausing
{
    public partial class BlurBackground : TextureRect
    {
        // Nodes
        private Tween _Tween;
        private ShaderMaterial _ShaderMaterial;
        private Callable _SetLod;
        private Callable _SetDistort;
        private ImageTexture _BgImage;

        public override void _Ready()
        {
            _ShaderMaterial = (ShaderMaterial)Material;
            _SetLod = new Callable(this, nameof(SetLod));
            _SetDistort = new Callable(this, nameof(SetDistortStrength));
        }

        public void FadeIn()
        {
            // Set the screenshot
            _BgImage = new ImageTexture();
            _BgImage.SetImage(GetViewport().GetTexture().GetImage());
            Texture = _BgImage;

            // Kill tween if it exists already
            if (_Tween != null) _Tween.Kill();
            // Create new tween
            _Tween = CreateTween()
                .SetParallel(true)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);

            // Tween in transparency
            _Tween.TweenProperty(this, "modulate:a", 1, 0.25f);
            // Tween in LoD in parallel
            var lod = (float)_ShaderMaterial.GetShaderParameter("lod");
            _Tween.TweenMethod(_SetLod, lod, 5, 0.25f);
            // Tween in distort strength after
            var distortStrength = (float)_ShaderMaterial.GetShaderParameter("distort_strength");
            _Tween.TweenMethod(_SetDistort, distortStrength, 1, 0.25f);
        }

        public void FadeOut()
        {
            // Kill tween if it exists already
            if (_Tween != null) _Tween.Kill();
            // Create new tween
            _Tween = CreateTween()
                .SetParallel(true)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);

            // Tween all values out in parallel
            var lod = (float)_ShaderMaterial.GetShaderParameter("lod");
            _Tween.TweenMethod(_SetLod, lod, 0, 0.25f);
            var distortStrength = (float)_ShaderMaterial.GetShaderParameter("distort_strength");
            _Tween.TweenMethod(_SetDistort, distortStrength, 0, 0.25f);
            _Tween.TweenProperty(this, "modulate:a", 0, 0.25f);
        }

        private void SetLod(float lod)
        {
            _ShaderMaterial.SetShaderParameter("lod", lod);
        }

        private void SetDistortStrength(float distortStrength)
        {
            _ShaderMaterial.SetShaderParameter("distort_strength", distortStrength);
        }
    }
}