using Godot;
using System;
using ShopIsDone.Game;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.GameSettings
{
    public partial class SettingsMenu : Control
    {
        [Signal]
        public delegate void ChangedTabEventHandler();

        [Signal]
        public delegate void CloseRequestedEventHandler();

        // Nodes
        // Settings Singleton
        private GameSettingsManager _GameSettings;

        // Tab container
        private TabContainer _TabContainer;
        private Button _CloseButton;

        // Audio
        private Control _AudioSettingsContainer;
        private HSlider _MasterVolume;
        private Label _MasterLabel;
        private HSlider _SfxVolume;
        private Label _SfxLabel;
        private HSlider _MusicVolume;
        private Label _MusicLabel;

        // Video
        private Control _VideoSettingsContainer;
        private CheckButton _Fullscreen;
        private OptionButton _Resolution;
        private ResolutionScaling _ResolutionScaling;
        private CheckButton _Vsync;
        private HSlider _UIScale;
        private Label _UIScaleLabel;

        // Debug
        private Control _DebugSettingsContainer;
        private CheckButton _ShowDebugDisplay;
        private CheckButton _BlurOnPause;

        public override void _Ready()
        {
            // Ready nodes
            // Settings Singleton
            _GameSettings = GameSettingsManager.GetGameSettings(this);

            // Tab Container
            _TabContainer = GetNode<TabContainer>("%TabContainer");
            _TabContainer.Connect("tab_selected", new Callable(this, nameof(OnTabSelected)));
            // Close button
            _CloseButton = GetNode<Button>("%CloseButton");
            _CloseButton.Connect("pressed", new Callable(this, nameof(OnClosePressed)));

            // Audio
            _AudioSettingsContainer = GetNode<Control>("%AudioSettingsContainer");
            _MasterVolume = GetNode<HSlider>("%MasterVolume");
            _MasterLabel = GetNode<Label>("%MasterLabel");
            _SfxVolume = GetNode<HSlider>("%SfxVolume");
            _SfxLabel = GetNode<Label>("%SfxLabel");
            _MusicVolume = GetNode<HSlider>("%MusicVolume");
            _MusicLabel = GetNode<Label>("%MusicLabel");
            // Connect
            _MasterVolume.Connect("value_changed", new Callable(this, nameof(OnMasterVolumeChanged)));
            _SfxVolume.Connect("value_changed", new Callable(this, nameof(OnSfxVolumeChanged)));
            _MusicVolume.Connect("value_changed", new Callable(this, nameof(OnMusicVolumeChanged)));

            // Video
            _VideoSettingsContainer = GetNode<Control>("%VideoSettingsContainer");
            _Fullscreen = GetNode<CheckButton>("%Fullscreen");
            _Resolution = GetNode<OptionButton>("%Resolution");
            _ResolutionScaling = GetNode<ResolutionScaling>("%ResolutionScaling");
            _Vsync = GetNode<CheckButton>("%Vsync");
            _UIScale = GetNode<HSlider>("%UIScale");
            _UIScaleLabel = GetNode<Label>("%UIScaleLabel");
            // Connect
            _Fullscreen.Toggled += _GameSettings.SetFullscreen;
            _Resolution.Connect("item_selected", new Callable(this, nameof(OnResolutionSelected)));
            _ResolutionScaling.ModeChanged += _GameSettings.SetResolutionScalingMode;
            _ResolutionScaling.ScaleChanged += _GameSettings.SetResolutionScaling;
            _UIScale.Connect("value_changed", new Callable(this, nameof(OnUIScaleChanged)));

            // Debug
            _DebugSettingsContainer = GetNode<Control>("%VideoSettingsContainer");
            _ShowDebugDisplay = GetNode<CheckButton>("%ShowDebugDisplay");
            _BlurOnPause = GetNode<CheckButton>("%BlurDuringPause");
            // Connect
            _ShowDebugDisplay.Toggled += _GameSettings.SetDebugDisplayVisible;
            // NB: Because this value can change with a button press, we should
            // connect this UI element to its change event
            _GameSettings.ShowDebugDisplayChanged += OnSettingsDebugDisplayChanged;
            _BlurOnPause.Toggled += _GameSettings.SetBlurDuringPause;

            // Hide Debug if not a debug build
            if (!GameManager.IsDebugMode())
            {
                // Get debug tab idx
                var debugTab = _DebugSettingsContainer.GetParent();
                var debugTabIdx = _TabContainer.GetTabIdxByTitle(debugTab.Name);

                // Disable and hide that tab
                _TabContainer.SetTabDisabled(debugTabIdx, true);
                _TabContainer.SetTabHidden(debugTabIdx, true);
            }
        }

        private void OnSettingsDebugDisplayChanged(bool newValue)
        {
            _ShowDebugDisplay.SetPressedNoSignal(newValue);
        }

        public void InitializeValues()
        {
            // Audio
            var master = _GameSettings.GetMasterVolume();
            _MasterVolume.Value = master;
            _MasterLabel.Text = master.ToString();

            var sfx = _GameSettings.GetSfxVolume();
            _SfxVolume.Value = sfx;
            _SfxLabel.Text = sfx.ToString();

            var music = _GameSettings.GetMusicVolume();
            _MusicVolume.Value = music;
            _MusicLabel.Text = music.ToString();

            // Video
            _Fullscreen.SetPressedNoSignal(_GameSettings.GetFullscreen());
            // Set resolutions
            var currentResolution = _GameSettings.GetResolution();
            var selectedIdx = -1;
            var allResolutions = _GameSettings.GetAvailableResolutions();
            _Resolution.Clear();
            for (int i = 0; i < allResolutions.Count; i++)
            {
                var resolution = allResolutions[i];
                var text = resolution.X + " x " + resolution.Y;
                _Resolution.AddItem(text, i);
                _Resolution.SetItemMetadata(i, resolution);

                // Find selected idx while we're here
                if (resolution == currentResolution) selectedIdx = i;
            }
            // Select current resolution
            _Resolution.Select(selectedIdx);
            // Set Scaling
            _ResolutionScaling.Init(
                _GameSettings.GetResolutionScaling(),
                (int)_GameSettings.GetResolutionScalingMode()
            );
            // Set Vsync
            _Vsync.SetPressedNoSignal(_GameSettings.GetVsync());
            // UI Scale
            var uiScale = _GameSettings.GetUIScaling();
            _UIScale.SetValueNoSignal(uiScale);
            _UIScaleLabel.Text = $"{uiScale}x";

            // Debug
            _ShowDebugDisplay.SetPressedNoSignal(_GameSettings.GetIsDebugDisplayVisible());
            _BlurOnPause.SetPressedNoSignal(_GameSettings.GetBlurDuringPause());
        }

        public Control GetFocusContainer()
        {
            return _TabContainer.GetCurrentTabControl();
        }

        public void SaveSettings()
        {
            _GameSettings.Save();
        }

        public override void _Process(double delta)
        {
            // Tab left and right
            if (Input.IsActionJustPressed("ui_cycle_tab_left"))
            {
                // Cycle
                if (_TabContainer.CurrentTab == 0)
                {
                    _TabContainer.CurrentTab = _TabContainer.GetTabCount() - 1;
                }
                else _TabContainer.CurrentTab -= 1;
                EmitSignal(nameof(ChangedTab));
            }

            if (Input.IsActionJustPressed("ui_cycle_tab_right"))
            {
                // Cycle
                if (_TabContainer.CurrentTab == _TabContainer.GetTabCount() - 1)
                {
                    _TabContainer.CurrentTab = 0;
                }
                else _TabContainer.CurrentTab += 1;
                EmitSignal(nameof(ChangedTab));
            }
        }

        // Hooks
        // Audio
        private void OnMasterVolumeChanged(int level)
        {
            _GameSettings.SetMasterVolume(level);
            _MasterLabel.Text = level.ToString();
        }

        private void OnSfxVolumeChanged(int level)
        {
            _GameSettings.SetSfxVolume(level);
            _SfxLabel.Text = level.ToString();
        }

        private void OnMusicVolumeChanged(int level)
        {
            _GameSettings.SetMusicVolume(level);
            _MusicLabel.Text = level.ToString();
        }

        // Video
        private void OnResolutionSelected(int idx)
        {
            var resolution = (Vector2)_Resolution.GetItemMetadata(idx);
            _GameSettings.SetResolution(resolution);
        }

        private void OnUIScaleChanged(float newValue)
        {
            _GameSettings.SetUIScaling(newValue);
            _UIScaleLabel.Text = $"{newValue}x";
        }

        // General
        private void OnTabSelected(int _)
        {
            EmitSignal(nameof(ChangedTab));
        }

        private void OnClosePressed()
        {
            EmitSignal(nameof(CloseRequested));
        }
    }
}