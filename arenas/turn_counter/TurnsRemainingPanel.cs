using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Arenas.Turns
{
    public partial class TurnsRemainingPanel : Node2D
    {
        [Signal]
        public delegate void CountdownBeganEventHandler();

        private TurnCountdownNumberStrip _FirstDigit;
        private TurnCountdownNumberStrip _SecondDigit;
        private TurnCountdownNumberStrip _ThirdDigit;
        private TurnCountdownNumberStrip _FourthDigit;
        private Sprite2D _PanelFrame;
        private Sprite2D _PanelFrameOverlay;

        // State
        private List<TurnCountdownNumberStrip> _Digits = new List<TurnCountdownNumberStrip>();
        private List<Vector2> _OriginalPosList = new List<Vector2>();
        private List<TurnCountdownNumberStrip> _TransitioningDigits = new List<TurnCountdownNumberStrip>();

        public override void _Ready()
        {
            _FirstDigit = GetNode<TurnCountdownNumberStrip>("%FirstDigit");
            _SecondDigit = GetNode<TurnCountdownNumberStrip>("%SecondDigit");
            _ThirdDigit = GetNode<TurnCountdownNumberStrip>("%ThirdDigit");
            _FourthDigit = GetNode<TurnCountdownNumberStrip>("%FourthDigit");
            _Digits = new List<TurnCountdownNumberStrip>()
        {
            _FirstDigit, _SecondDigit, _ThirdDigit, _FourthDigit
        };
            foreach (var digit in _Digits) _OriginalPosList.Add(digit.Position);

            _PanelFrame = GetNode<Sprite2D>("%PanelFrame");
            _PanelFrameOverlay = GetNode<Sprite2D>("%PanelFrameOverlay");
        }

        public void ResetTurnPanel()
        {
            for (int i = 0; i < _Digits.Count; i++)
            {
                _Digits[i].Position = _OriginalPosList[i];
            }
        }

        public void CountdownTurns()
        {
            // Emit countdown began
            EmitSignal(nameof(CountdownBegan));

            // Create tween
            var tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);
            for (int k = 0; k < _TransitioningDigits.Count; k++)
            {
                var digit = _TransitioningDigits[k];
                var finalPos = new Vector2(digit.Position.X, digit.Position.Y + 124);
                tween
                    .Parallel()
                    .TweenProperty(_TransitioningDigits[k], "position", finalPos, 0.5f)
                    .SetDelay(k * .15f);
            }
        }

        public void SetTurnsRemaining(int turns, int maxTurns)
        {
            // Clear list of transitioning digits
            _TransitioningDigits.Clear();

            // Create turn strings to compare against
            var turnString = turns.ToString().PadZeros(4);
            var prevTurn = turns + 1;
            var prevTurnString = prevTurn.ToString().PadZeros(4);

            // Set previous digits
            for (int i = 0; i < _Digits.Count; i++)
            {
                _Digits[i].SetBottomNumber(prevTurnString[i]);
            }

            // Handle Last Turn
            if (turns == 1)
            {
                _FirstDigit.SetTopNumber('l');
                _SecondDigit.SetTopNumber('a');
                _ThirdDigit.SetTopNumber('s');
                _FourthDigit.SetTopNumber('t');
                // Just copy the whole damn list
                _TransitioningDigits = _Digits.ToList();

                // Handle panel frame
                _PanelFrame.Frame = _PanelFrame.Hframes - 1;
                _PanelFrameOverlay.Hide();

            }
            // Otherwise, set them to the proper next digit
            else
            {
                for (int j = 0; j < _Digits.Count; j++)
                {
                    _Digits[j].SetTopNumber(turnString[j]);
                    // If the two turn strings in that position aren't equal,
                    // set the digit to be transitioned
                    if (prevTurnString[j] != turnString[j]) _TransitioningDigits.Add(_Digits[j]);
                }

                // Handle panel frame
                // Subtract 2 frames because the last frame is the "last" panel
                var numFrames = (float)(_PanelFrame.Hframes - 2);
                var shifted = ((float)(maxTurns - turns)).ShiftRange(0, maxTurns, 0, numFrames);
                var currentFrame = Mathf.FloorToInt(shifted);

                // Set panel and overlay frame
                _PanelFrame.Frame = currentFrame;
                _PanelFrameOverlay.Frame = currentFrame + 1;

                // Set panel overlay opacity based on percent completion between chunks
                var chunkSize = Mathf.CeilToInt(maxTurns / numFrames);
                var panelOpacity = Colors.Transparent;
                panelOpacity.A = 1f - ((turns % chunkSize) / (float)chunkSize);
                _PanelFrameOverlay.Modulate = panelOpacity;
                // Show just incase it's hidden
                _PanelFrameOverlay.Show();
            }
        }
    }
}