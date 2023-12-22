using Godot;
using System;

namespace ShopIsDone.GameSettings
{
	public partial class ResolutionScaling : VBoxContainer
	{
		[Signal]
		public delegate void ScaleChangedEventHandler(float scale);

        [Signal]
        public delegate void ModeChangedEventHandler(int mode);

        private Control _ResolutionScalingContainer;
        private Label _ResolutionScalingLabel;
        public Button _ResetScalingButton;
		private Slider _ResolutionScaling;
        private Control _QualityContainer;
		private OptionButton _Quality;
		private OptionButton _ScalingMode;

		public override void _Ready()
		{
            // Scaling
            _ResolutionScalingContainer = GetNode<Control>("%ResolutionScalingContainer");
            _ResolutionScalingLabel = GetNode<Label>("%ResolutionScalingLabel");
            _ResolutionScaling = GetNode<Slider>("%ResolutionScaling");
            _ResolutionScaling.Connect("value_changed", new Callable(this, nameof(OnResolutionScalingChanged)));

            // Reset scaling
            _ResetScalingButton = GetNode<Button>("%ResetScalingButton");
            _ResetScalingButton.Pressed += OnResetScaling;

            // FSR Quality
            _QualityContainer = GetNode<Control>("%QualityContainer");
            _Quality = GetNode<OptionButton>("%Quality");
            // "When using FSR upscaling, AMD recommends exposing the following
            // values as preset options to users "Ultra Quality: 0.77", "Quality:
            // 0.67", "Balanced: 0.59", "Performance: 0.5" instead of exposing
            // the entire scale.
            _Quality.SetItemMetadata(0, 50);
            _Quality.SetItemMetadata(1, 59);
            _Quality.SetItemMetadata(2, 67);
            _Quality.SetItemMetadata(3, 77);
            _Quality.Selected = -1;
            _Quality.Connect("item_selected", new Callable(this, nameof(OnQualitySelected)));

            // Mode
            _ScalingMode = GetNode<OptionButton>("%ScalingMode");
            _ScalingMode.SetItemMetadata(0, (int)Viewport.Scaling3DModeEnum.Bilinear);
            _ScalingMode.SetItemMetadata(1, (int)Viewport.Scaling3DModeEnum.Fsr2);
            _ScalingMode.Selected = -1;
            _ScalingMode.Connect("item_selected", new Callable(this, nameof(OnScalingModeChanged)));
        }

        public void Init(int scale, int mode)
		{
            // Choose which state we're in based on the mode
            if (mode == (int)Viewport.Scaling3DModeEnum.Bilinear)
            {
                _ScalingMode.Selected = 0;
                EnterBilinear(scale);
            }
            else
            {
                _ScalingMode.Selected = 1;
                if (scale == 50) _Quality.Select(0);
                else if (scale == 59) _Quality.Select(1);
                else if (scale == 67) _Quality.Select(2);
                else if (scale == 77) _Quality.Select(3);
                EnterFsr();
            }
        }

		private void EnterFsr()
		{
            // Show
            _QualityContainer.Show();
            _ResolutionScalingContainer.Hide();

            // Change focus on scaling mode
            _ScalingMode.FocusPrevious = _Quality.GetPath();
        }

		private void EnterBilinear(int scale)
		{
            // Show
            _QualityContainer.Hide();
            _ResolutionScalingContainer.Show();

            // Change focus on scaling mode
            _ScalingMode.FocusPrevious = _ResetScalingButton.GetPath();

            // Ensure
            _ResolutionScaling.SetValueNoSignal(scale);
            SetScaleLabel(scale);
        }

        private void OnScalingModeChanged(int idx)
        {
            // Pull mode from metadata
            var mode = (int)_ScalingMode.GetItemMetadata(idx);

            // Change view
            if (mode == (int)Viewport.Scaling3DModeEnum.Bilinear)
            {
                // Change scaling back to 100
                OnResolutionScalingChanged(100);
                // Update view
                EnterBilinear(100);
            }
            else
            {
                // Pick "Balanced" quality value
                _Quality.Selected = 1;
                OnQualitySelected(1);
                // Update view
                EnterFsr();
            }

            // Emit signal
            EmitSignal(nameof(ModeChanged), mode);
        }

        private void SetScaleLabel(float value)
        {
            _ResolutionScalingLabel.Text = ((int)value).ToString() + "%";
        }

        private void OnResolutionScalingChanged(float value)
        {
            SetScaleLabel(value);
            EmitSignal(nameof(ScaleChanged), value);
        }

        private void OnQualitySelected(int idx)
        {
            // Pull mode from metadata
            var quality = (float)_Quality.GetItemMetadata(idx);
            // Emit
            EmitSignal(nameof(ScaleChanged), quality);
        }

        private void OnResetScaling()
        {
            _ResolutionScaling.SetValueNoSignal(100);
            OnResolutionScalingChanged(100);
        }
    }
}
