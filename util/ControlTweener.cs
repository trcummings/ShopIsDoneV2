using Godot;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.Utils.UI
{
    public partial class ControlTweener : Node
    {
        [Signal]
        public delegate void FinishedEventHandler();

        [Export]
        private Control _ControlToTween;

        [Export]
        public Tween.EaseType InEaseType = Tween.EaseType.Out;

        [Export]
        public Tween.TransitionType InTransType = Tween.TransitionType.Bounce;

        [Export]
        public ScreenPos InPos = ScreenPos.Top;

        [Export]
        public Tween.EaseType OutEaseType = Tween.EaseType.In;

        [Export]
        public Tween.TransitionType OutTransType = Tween.TransitionType.Bounce;

        [Export]
        public ScreenPos OutPos = ScreenPos.Top;

        public enum ScreenPos
        {
            Top, Bottom, Left, Right
        }

        public void TweenIn(float duration)
        {
            Task _ = TweenInAsync(duration);
        }

        public void TweenOut(float duration)
        {
            Task _ = TweenOutAsync(duration);
        }

        public async Task TweenInAsync(float duration)
        {
            // Set our rect position off frame by the full vertical size of the rect
            var frameStartPos = _ControlToTween.Position - new Vector2(0, _ControlToTween.Size.Y);
            var frameEndPos = _ControlToTween.Position;

            // Set the frame to be off screen
            _ControlToTween.Position = frameStartPos;

            // Show frame container
            _ControlToTween.Show();

            // Create tween
            var tween = GetTree().CreateTween();

            // Tween Frame in
            tween
                .BindNode(this)
                .TweenProperty(_ControlToTween, "position", frameEndPos, duration)
                // Set ease and trans type
                .SetEase(InEaseType)
                .SetTrans(InTransType);

            // Await finish
            await ToSignal(tween, "finished");

            // Emit
            EmitSignal(nameof(Finished));
        }

        public async Task TweenOutAsync(float duration)
        {
            // Set our rect position off frame by the full vertical size of the rect
            var (frameStartPos, frameEndPos) = GetFrameOutPos();

            // Create tween
            var tween = GetTree().CreateTween();

            // Tween Frame out
            tween
                .BindNode(this)
                .TweenProperty(_ControlToTween, "position", frameEndPos, duration)
                // Set ease and trans type
                .SetEase(OutEaseType)
                .SetTrans(OutTransType);

            // Await finish
            await ToSignal(tween, "finished");

            // Hide frame container
            _ControlToTween.Hide();
            // Reset position
            _ControlToTween.Position = frameStartPos;

            // Emit
            EmitSignal(nameof(Finished));
        }

        private (Vector2, Vector2) GetFrameOutPos()
        {
            // Top case
            if (OutPos == ScreenPos.Top)
            {
                return (
                    _ControlToTween.Position,
                    _ControlToTween.Position - new Vector2(0, _ControlToTween.Size.Y)
                );
            }
            // Bottom case
            if (OutPos == ScreenPos.Bottom)
            {
                return (
                    _ControlToTween.Position,
                    _ControlToTween.Position + new Vector2(0, _ControlToTween.Size.Y)
                );
            }

            // Null case
            return (_ControlToTween.Position, _ControlToTween.Position);
        }
    }
}
