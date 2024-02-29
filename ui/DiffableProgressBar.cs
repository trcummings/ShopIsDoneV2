using Godot;
using System;

namespace ShopIsDone.UI
{
    [Tool]
    public partial class DiffableProgressBar : Control
    {
        [Export]
        public bool ShowDiff
        {
            get { return _ShowDiff; }
            set { SetShowDiff(value); }
        }
        private bool _ShowDiff = false;

        [Export]
        public int MaxValue
        {
            get { return _MaxValue; }
            set { SetMaxValue(value); }
        }
        private int _MaxValue = 1;

        [Export]
        public int Value
        {
            get { return _Value; }
            set { SetValue(value); }
        }
        private int _Value = 0;

        [Export]
        public int DiffValue
        {
            get { return _DiffValue; }
            set { SetDiffValue(value); }
        }
        private int _DiffValue = 0;

        // Nodes
        private ProgressBar _BaseBar;
        private ProgressBar _OverlayBar;
        private Label _Label;
        private AnimationPlayer _AnimPlayer;

        public override void _Ready()
        {
            _BaseBar = GetNode<ProgressBar>("%BaseBar");
            _OverlayBar = GetNode<ProgressBar>("%OverlayBar");
            _Label = GetNode<Label>("%Label");
            _AnimPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

            SetShowDiff(ShowDiff);
            SetMaxValue(MaxValue);
        }

        private void SetShowDiff(bool value)
        {
            _ShowDiff = value;

            if (value) _OverlayBar?.Show();
            else _OverlayBar?.Hide();
            SetLabel();
            SetStrobe();
        }

        private void SetMaxValue(int maxValue)
        {
            _MaxValue = maxValue;
            // Set the bar values
            _BaseBar?.SetValueNoSignal(GetBarValue(_Value));
            _OverlayBar?.SetValueNoSignal(GetBarValue(_DiffValue));
            SetLabel();
            SetStrobe();
        }

        private void SetValue(int value)
        {
            _Value = value;
            _BaseBar?.SetValueNoSignal(GetBarValue(_Value));
            SetLabel();
            SetStrobe();
        }

        private void SetDiffValue(int value)
        {
            _DiffValue = value;
            _OverlayBar?.SetValueNoSignal(GetBarValue(_DiffValue));
            SetLabel();
            SetStrobe();
        }

        private void SetLabel()
        {
            if (_Label == null) return;
            if (ShowDiff)
            {
                // Show the text as a diff amount
                _Label.Text = $"{_Value} -> {_DiffValue} / {_MaxValue}";
            }
            else
            {
                // Show the text as a normal amount
                _Label.Text = $"{_Value} / {_MaxValue}";
            }
        }

        private void SetStrobe()
        {
            if (_Value > _DiffValue) _AnimPlayer?.Play("strobe_base");
            else _AnimPlayer?.Play("strobe_overlay");
        }

        private float GetBarValue(int value)
        {
            return value / (float)MaxValue * 100;
        }
    }
}

