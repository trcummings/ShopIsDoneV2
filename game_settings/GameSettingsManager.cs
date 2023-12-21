using Godot;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Game;

namespace ShopIsDone.GameSettings
{
    public partial class GameSettingsManager : Node
    {
        // Video Signals
        [Signal]
        public delegate void ResolutionChangedEventHandler(Vector2 newResolution);

        [Signal]
        public delegate void ResolutionScaleChangedEventHandler(float newScale);

        [Signal]
        public delegate void VsyncChangedEventHandler(bool newValue);

        [Signal]
        public delegate void ScalingModeChangedEventHandler(int newValue);

        // Debug Signals
        [Signal]
        public delegate void BlurDuringPauseChangedEventHandler(bool newValue);

        [Signal]
        public delegate void ShowDebugDisplayChangedEventHandler(bool newValue);

        // State
        private ConfigFile _DefaultSettings = new ConfigFile();
        private ConfigFile _UserSettings = new ConfigFile();

        // Static function to help get the singleton
        public static GameSettingsManager GetGameSettings(Node node)
        {
            return node.GetNode<GameSettingsManager>("/root/GameSettingsManager");
        }

        public void Load()
        {
            // Load in default settings
            _DefaultSettings.Load(Consts.DefaultsPath);

            // Load in user settings
            var error = _UserSettings.Load(Consts.UserSettingsPath);
            // If there's an error loading the settings, reset it to the defaults
            if (error != Error.Ok) ResetToDefaults();
            // Otherwise, make sure we copy over any defaults we might be missing
            else CopyOverMissingValues();
        }

        public void InitValues()
        {
            SetFullscreen(GetFullscreen());
            SetResolution(GetResolution());
            SetResolutionScaling(GetResolutionScaling());
            SetResolutionScalingMode(GetResolutionScalingMode());
            SetVsync(GetVsync());
            // Audio Settings
            SetMasterVolume(GetMasterVolume());
            SetSfxVolume(GetSfxVolume());
            SetMusicVolume(GetMusicVolume());
            // Debug Settings
            SetDebugDisplayVisible(GetIsDebugDisplayVisible());
            SetBlurDuringPause(GetBlurDuringPause());
        }

        public void Save()
        {
            _UserSettings.Save(Consts.UserSettingsPath);
        }

        // Video
        public bool GetFullscreen()
        {
            return (bool)_UserSettings.GetValue("video", "fullscreen", false);
        }

        public void SetFullscreen(bool value)
        {
            // Set the values
            _UserSettings.SetValue("video", "fullscreen", value);
            var window = GetWindow();
            window.Mode = value ? Window.ModeEnum.Fullscreen : Window.ModeEnum.Windowed;

            // Upadate the window size
            UpdateWindowSize();
        }

        public List<Vector2> GetAvailableResolutions()
        {
            // Detect the screen size
            var window = GetWindow();
            var screenSize = window.Size;

            // Get the raw resolutions and convert to a list
            var rawResolutions = (Godot.Collections.Array)_UserSettings.GetValue("video", "_resolutions", new Godot.Collections.Array());
            var resolutions = new List<Vector2>();
            foreach (var resolution in rawResolutions) resolutions.Add((Vector2)resolution);

            // Filter down by available sizes
            return resolutions
                .Where(resolution => resolution.X <= screenSize.X && resolution.Y <= screenSize.Y)
                // To list
                .ToList();
        }

        public Vector2 GetResolution()
        {
            var projectResolution = GameManager.GetProjectViewportSize();
            return (Vector2)_UserSettings.GetValue("video", "resolution", projectResolution);
        }

        public void SetResolution(Vector2 resolution)
        {
            // TODO: check if resolution is allowed and if not, get the closest resolution

            // Set new value in settings / viewport
            _UserSettings.SetValue("video", "resolution", resolution);

            // Emit signal
            EmitSignal(nameof(ResolutionChanged), resolution);

            // Update the window size
            UpdateWindowSize();
        }

        private void UpdateWindowSize()
        {
            // Adjust the window size if not fullscreen
            if (!GetFullscreen())
            {
                // Set window size to resolution size
                var resolution = GetResolution();
                var window = GetWindow();
                window.Size = new Vector2I((int)resolution.X, (int)resolution.Y);

                // Get center of screen
                var centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
                // Set the window at the center
                window.Position = centerScreen;
            }
        }

        public int GetResolutionScaling()
        {
            return (int)_UserSettings.GetValue("video", "resolution_scale", 100);
        }

        public void SetResolutionScaling(int scale)
        {
            // Set value in settings
            _UserSettings.SetValue("video", "resolution_scale", scale);

            // Update value in viewport
            GetViewport().Scaling3DScale = scale;

            // Emit signal
            EmitSignal(nameof(ResolutionScaleChanged), scale);
        }

        public Window.Scaling3DModeEnum GetResolutionScalingMode()
        {
            return (Window.Scaling3DModeEnum)(int)_UserSettings.GetValue("video", "scaling_mode", (int)Window.Scaling3DModeEnum.Bilinear);
        }

        public void SetResolutionScalingMode(Window.Scaling3DModeEnum scalingMode)
        {
            // Set value in settings
            _UserSettings.SetValue("video", "scaling_mode", (int)scalingMode);

            // Set scaling in viewport
            GetViewport().Scaling3DMode = scalingMode;

            // Emit signal
            EmitSignal(nameof(ScalingModeChanged), (int)scalingMode);
        }

        public bool GetVsync()
        {
            return (bool)_UserSettings.GetValue("video", "vsync", false);
        }

        public void SetVsync(bool newValue)
        {
            // Update value in settings
            _UserSettings.SetValue("video", "vsync", newValue);

            // Update the value on the display server
            DisplayServer.WindowSetVsyncMode(newValue ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);

            // Emit signal
            EmitSignal(nameof(VsyncChanged), newValue);
        }

        // Audio
        public int GetMasterVolume()
        {
            return (int)_UserSettings.GetValue("audio", "master", 50);
        }

        public void SetMasterVolume(int level)
        {
            SetVolume("master", level, AudioServer.GetBusIndex("Master"));
        }

        public int GetSfxVolume()
        {
            return (int)_UserSettings.GetValue("audio", "sfx", 50);
        }

        public void SetSfxVolume(int level)
        {
            SetVolume("sfx", level, AudioServer.GetBusIndex("SFX"));
        }

        public int GetMusicVolume()
        {
            return (int)_UserSettings.GetValue("audio", "music", 50);
        }

        public void SetMusicVolume(int level)
        {
            SetVolume("music", level, AudioServer.GetBusIndex("Music"));
        }

        private void SetVolume(string key, int level, int busIdx)
        {
            // Set value in settings
            _UserSettings.SetValue("audio", key, level);

            // Convert to DB
            var percent = level / 100f;
            var db = Mathf.LinearToDb(percent);

            // Set on audio server
            AudioServer.SetBusVolumeDb(busIdx, db);
        }

        // Debug
        public bool GetIsDebugDisplayVisible()
        {
            return (bool)_UserSettings.GetValue("debug", "show_debug_display", false);
        }

        public void SetDebugDisplayVisible(bool value)
        {
            _UserSettings.SetValue("debug", "show_debug_display", value);

            // Emit signal
            EmitSignal(nameof(ShowDebugDisplayChanged), value);
        }

        public bool GetBlurDuringPause()
        {
            return (bool)_UserSettings.GetValue("debug", "blur_during_pause", true);
        }

        public void SetBlurDuringPause(bool value)
        {
            _UserSettings.SetValue("debug", "blur_during_pause", value);

            // Emit signal
            EmitSignal(nameof(BlurDuringPauseChanged), value);
        }


        // Utility Functions
        private void CopyOverMissingValues()
        {
            // Loop over the default settings and fill in any missing user settings
            // values
            foreach (var section in _DefaultSettings.GetSections())
            {
                foreach (var key in _DefaultSettings.GetSectionKeys(section))
                {
                    // Copy over missing key value pairs, and also keys that begin with _
                    if (!_UserSettings.HasSectionKey(section, key) || key.StartsWith("_"))
                    {
                        var defaultValue = _DefaultSettings.GetValue(section, key);
                        _UserSettings.SetValue(section, key, defaultValue);
                    }
                }
            }

            // Save the user settings
            _UserSettings.Save(Consts.UserSettingsPath);
        }

        private void ResetToDefaults()
        {
            // Clear out the user settings
            _UserSettings.Clear();

            // Loop over the default settings and set the user settings values to
            // each one
            foreach (var section in _DefaultSettings.GetSections())
            {
                foreach (var key in _DefaultSettings.GetSectionKeys(section))
                {
                    var defaultValue = _DefaultSettings.GetValue(section, key);
                    _UserSettings.SetValue(section, key, defaultValue);
                }
            }

            // Save the user settings
            _UserSettings.Save(Consts.UserSettingsPath);
        }
    }
}