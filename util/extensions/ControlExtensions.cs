using System;
using System.Threading.Tasks;
using Godot;

namespace ShopIsDone.Utils.Extensions
{
	public static class ControlExtensions
	{
        public static async Task FadeIn(
            this Control control,
            float duration,
            Tween.EaseType ease = Tween.EaseType.Out,
            Tween.TransitionType trans = Tween.TransitionType.Linear
        )
        {
            control.Modulate = Colors.Transparent;
            control.Show();

            // Tween in icon
            var tween = control
                .GetTree()
                .CreateTween()
                .TweenProperty(control, "modulate:a", 1f, duration)
                // Set ease and trans type
                .SetEase(ease)
                .SetTrans(trans);

            await control.ToSignal(tween, "finished");
        }

        public static async Task FadeOut(
            this Control control,
            float duration,
            Tween.EaseType ease = Tween.EaseType.Out,
            Tween.TransitionType trans = Tween.TransitionType.Linear
        )
        {
            // Tween out
            var tween = control
                .GetTree()
                .CreateTween()
                .TweenProperty(control, "modulate:a", 0f, duration)
                // Set ease and trans type
                .SetEase(ease)
                .SetTrans(trans);
            await control.ToSignal(tween, "finished");

            control.Hide();
        }
    }
}

