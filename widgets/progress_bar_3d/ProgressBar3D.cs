using Godot;
using System;
using System.Threading.Tasks;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Widgets
{
    public partial class ProgressBar3D : Node3D
    {
        public float BarValue
        {
            get { return GetBarValue(); }
            set { SetBarValue(value); }
        }

        private float GetBarValue()
        {
            return (float)_ProgressBar?.Value;
        }

        private void SetBarValue(float value)
        {
            if (_ProgressBar != null) _ProgressBar.Value = value;
        }

        [Export]
        private ProgressBar _ProgressBar;

        public void TweenValue(float value)
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_ProgressBar, "value", value, 0.15f)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.OutIn);
        }

        public void FadeIn(float duration)
        {
            Task _ = _ProgressBar.FadeIn(duration);
        }

        public void FadeOut(float duration)
        {
            Task _ = _ProgressBar.FadeOut(duration);
        }
    }
}

