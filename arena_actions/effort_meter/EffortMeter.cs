using Godot;
using ShopIsDone.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.Actions.Effort
{
    public partial class EffortMeter : Control
    {
        [Signal]
        public delegate void ActivatedEventHandler();

        [Signal]
        public delegate void DeactivatedEventHandler();

        [Signal]
        public delegate void IncrementedEventHandler();

        [Signal]
        public delegate void DecrementedEventHandler();

        [Signal]
        public delegate void InvalidSelectionEventHandler();

        private List<Label> _Numbers = new List<Label>();
        private TextureRect _SaturatedMeter;
        private Control _PointerContainer;
        private MarginContainer _Pointer;
        private TextureRect _EffortSmiley;
        private Control _ActivateMeterIcon;
        private Label _AdjustEffortLabel;

        public int CurrentIndex { get { return _CurrentIndex; } }

        // State
        private int _CurrentIndex = 0;
        private int _MaxIdx = 5;

        public override void _Ready()
        {
            // Ready nodes
            _SaturatedMeter = GetNode<TextureRect>("%SaturatedMeter");
            _PointerContainer = GetNode<Control>("%PointerContainer");
            _Pointer = GetNode<MarginContainer>("%Pointer");
            _EffortSmiley = GetNode<TextureRect>("%EffortSmiley");
            _ActivateMeterIcon = GetNode<Control>("%ActivateMeterIcon");
            _AdjustEffortLabel = GetNode<Label>("%AdjustEffortLabel");
            _Numbers = GetNode<Control>("%NumbersContainer")
                .GetChildren()
                .OfType<Label>()
                .ToList();
            // Set the number values
            for (int i = 0; i < _Numbers.Count; i++)
            {
                var number = _Numbers[i];
                number.Text = i.ToString();
            }
        }

        public void Init(int value, int maxValue = 5)
        {
            _MaxIdx = maxValue;
            _CurrentIndex = Mathf.Min(value, maxValue);
            // Set slider to appropriate value
            SetMeterTo(_CurrentIndex);
            MoveSliderToIndex(0);
        }

        public void Activate()
        {
            // Hide the activate icon, and show the effort smiley
            SetSmiley(_CurrentIndex);
            _EffortSmiley.Show();
            _ActivateMeterIcon.Hide();
            _AdjustEffortLabel.Hide();

            // Slide pointer container to current index
            MoveSliderToIndex(_CurrentIndex);

            // Slide pointer down
            SlideCursorTo(45);

            EmitSignal(nameof(Activated));
        }

        public void Deactivate()
        {
            // Show the activate icon, and hide the effort smiley
            _EffortSmiley.Hide();
            _ActivateMeterIcon.Show();
            _AdjustEffortLabel.Show();

            // Slide pointer container to zero
            MoveSliderToIndex(0);

            // Slide pointer back up
            SlideCursorTo(0);

            EmitSignal(nameof(Deactivated));
        }

        public void Increment()
        {
            // Track the previous value, if it's the same, action is invalid
            var prevIdx = _CurrentIndex;
            _CurrentIndex = Mathf.Min(_CurrentIndex + 1, _MaxIdx);
            // Invalid case
            if (prevIdx == _CurrentIndex)
            {
                EmitSignal(nameof(InvalidSelection));
                return;
            }

            // Otherwise, increment
            MoveSliderToIndex(_CurrentIndex);
            SetSmiley(_CurrentIndex);
            SetMeterTo(_CurrentIndex);
            EmitSignal(nameof(Incremented));
        }

        public void Decrement()
        {
            // Track the previous value, if it's the same, action is invalid
            var prevIdx = _CurrentIndex;
            _CurrentIndex = Mathf.Max(_CurrentIndex - 1, 0);

            // Invalid case
            if (prevIdx == _CurrentIndex)
            {
                EmitSignal(nameof(InvalidSelection));
                return;
            }

            MoveSliderToIndex(_CurrentIndex);
            SetSmiley(_CurrentIndex);
            SetMeterTo(_CurrentIndex);
            EmitSignal(nameof(Decremented));
        }

        // Tweens and other modifications
        private void MoveSliderToIndex(int idx)
        {
            var numberWidth = (_Numbers.First().GetParent() as Control).Size.X / _Numbers.Count;
            var finalPos = new Vector2(
                idx * numberWidth,
                -25
            );
            CreateTween()
                .TweenProperty(_PointerContainer, "position", finalPos, 0.1f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);
        }

        private void SetNumberSelected(int index)
        {
            // Set the number values
            for (int i = 0; i < _Numbers.Count; i++)
            {
                var number = _Numbers[i];
                if (i == index) number.ThemeTypeVariation = null;
                else number.ThemeTypeVariation = "LabelDark";
            }
        }

        private void SetSmiley(int index)
        {
            // Set smiley region on atlas
            (_EffortSmiley.Texture as AtlasTexture).SetAtlasIdx(index, 6);
        }

        private void SetMeterTo(int index)
        {
            var fraction = 1f / 6f;
            var toPercent = Mathf.Min((index + 1f) * fraction, 1f);
            var fromPercent = (_SaturatedMeter.Material as ShaderMaterial).GetShaderParameter("percentage");
            CreateTween()
                .TweenMethod(new Callable(this, nameof(SetPercentage)), fromPercent, toPercent, 0.1f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);
            SetNumberSelected(index);
        }

        private void SetPercentage(float percent)
        {
            (_SaturatedMeter.Material as ShaderMaterial).SetShaderParameter("percentage", percent);
        }

        private void SetPointerMarginTop(int amount)
        {
            _Pointer.AddThemeConstantOverride("margin_top", amount);
        }

        private void SetPointerMarginBottom(int amount)
        {
            _Pointer.AddThemeConstantOverride("margin_bottom", amount);
        }

        private void SlideCursorTo(int outAmount)
        {
            var pointerTween = CreateTween();
            var initialMarginTop = _Pointer.GetThemeConstant("margin_top");
            var initialMarginBottom = _Pointer.GetThemeConstant("margin_bottom");
            pointerTween
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic)
                .TweenMethod(new Callable(this, nameof(SetPointerMarginTop)), initialMarginTop, outAmount, 0.05f);
            pointerTween
                .Parallel()
                .TweenMethod(new Callable(this, nameof(SetPointerMarginBottom)), initialMarginBottom, -outAmount, 0.05f);
        }
    }
}