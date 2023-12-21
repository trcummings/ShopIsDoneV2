using Godot;
using ShopIsDone.GameSettings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShopIsDone.Game
{
    public partial class GameManager : Node
    {
        [Export(PropertyHint.Enum, "Vanity Card, Main Menu, Level, Level Editor, Debug Level Select, Break Room")]
        public int OverrideModeAfterLoad = 1;

        [Export(PropertyHint.Dir)]
        public string InitialLevelFolderPath;

        // Enum for what game mode should be picked
        public enum InitialGameState
        {
            VanityCard = 0,
            MainMenu = 1,
            Level = 2,
            LevelEditor = 3,
            DebugLevelSelect = 4,
            BreakRoom = 5
        }

        // Nodes
        //private StateMachine _GSM;
        private WorldEnvironment _WorldEnvironment;
        private ColorRect _BlackOverlay;
        //private DebugDisplay _DebugDisplay;
        private GameSettingsManager _GameSettings;

        public override void _Ready()
        {
            // Ready nodes
            //_GSM = GetNode<StateMachine>("%GameStateMachine");
            _WorldEnvironment = GetNode<WorldEnvironment>("%WorldEnvironment");
            _BlackOverlay = GetNode<ColorRect>("%BlackOverlay");
            //_DebugDisplay = GetNode<DebugDisplay>("%DebugDisplay");
            _GameSettings = GameSettingsManager.GetGameSettings(this);

            // Show black overlay
            _BlackOverlay.Color = Colors.Black;
            _BlackOverlay.Show();

            //// Connect to settings events
            //_GameSettings.Connect(nameof(GameSettings.ShowDebugDisplayChanged), this, nameof(SetDebugDisplayVisibility));
            //_GameSettings.Connect(nameof(GameSettings.ResolutionChanged), this, nameof(SetRootViewportSize));
            //_GameSettings.Connect(nameof(GameSettings.ResolutionScaleChanged), this, nameof(OnResolutionScaleChanged));
            //_GameSettings.Connect(nameof(GameSettings.FxaaChanged), this, nameof(OnFxaaChanged));
            //_GameSettings.Connect(nameof(GameSettings.MsaaChanged), this, nameof(OnMsaaChanged));

            //// Connect to whenever a node is added to the scene (for viewport filtering)
            //GetTree().Connect("node_added", this, nameof(OnPotentialViewportAdded));

            //// Init Game states with given vars
            //var states = _GSM.GetChildren().OfType<GameState>();
            //foreach (var state in states) state.Init(this);

            //// Set GSM into settings load state
            //_GSM.ChangeState("InitialLoadGameState");
        }


        public override void _Process(double delta)
        {
            // Hit Esc to cancel at any time
            // FIXME: Remove once there's a quit menu in place
            if (Input.IsActionJustPressed("escape")) QuitGame();

            // Check for debug console input
            if (Input.IsActionJustPressed("debug_console") && IsDebugMode())
            {
                // Toggle debug display visibility
                _GameSettings.SetDebugDisplayVisible(!_GameSettings.GetIsDebugDisplayVisible());
            }
        }

        // Quit
        public void QuitGame()
        {
            // Save settings
            _GameSettings.Save();

            // Quit game
            GetTree().Quit();
        }


        // World environment
        public void SetEnvironment(Godot.Environment environment)
        {
            _WorldEnvironment.Environment = environment;
        }

        // Game Settings
        public void LoadInitialGameSettings()
        {
            // Load in settings
            _GameSettings.Load();
            // Video settings
            _GameSettings.SetFullscreen(_GameSettings.GetFullscreen());
            _GameSettings.SetResolution(_GameSettings.GetResolution());
            _GameSettings.SetResolutionScaling(_GameSettings.GetResolutionScaling());
            _GameSettings.SetFxaa(_GameSettings.GetFxaa());
            _GameSettings.SetMsaa(_GameSettings.GetMsaa());
            _GameSettings.SetVsync(_GameSettings.GetVsync());
            // Audio Settings
            _GameSettings.SetMasterVolume(_GameSettings.GetMasterVolume());
            _GameSettings.SetSfxVolume(_GameSettings.GetSfxVolume());
            _GameSettings.SetMusicVolume(_GameSettings.GetMusicVolume());
            // Debug Settings
            _GameSettings.SetDebugDisplayVisible(_GameSettings.GetIsDebugDisplayVisible());
            _GameSettings.SetBlurDuringPause(_GameSettings.GetBlurDuringPause());
        }

        // Debug Display
        private void SetDebugDisplayVisibility(bool value)
        {
            //_DebugDisplay.SetVisibility(value);
        }

        // Overlay
        public async Task FadeOutOverlay()
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_BlackOverlay, "color", new Color(0, 0, 0, 0), 0.5F);
            await ToSignal(tween, "finished");
        }

        public async Task FadeInOverlay()
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(_BlackOverlay, "color", Colors.Black, 0.5F);
            await ToSignal(tween, "finished");
        }

        // Viewport options
        private void SetRootViewportSize(Vector2 newSize)
        {
            // Get root viewport
            var rootViewport = GetViewport();
            // Set size
            //rootViewport.Size = newSize;
            //GetTree().SetScreenStretch(SceneTree.StretchMode.Viewport, SceneTree.StretchAspect.Keep, newSize);

            //// Set viewports that aren't contained in viewport containers
            //foreach (var viewport in GetTree().GetNodesInGroup(Consts.Groups.WorldViewport).OfType<Viewport>())
            //{
            //    viewport.Size = newSize;
            //}
        }

        private void OnResolutionScaleChanged(int _)
        {
            SetAllViewportsProperties();
        }

        private void OnFxaaChanged(bool _)
        {
            SetAllViewportsProperties();
        }

        private void OnMsaaChanged(int _)
        {
            SetAllViewportsProperties();
        }

        private void OnPotentialViewportAdded(Node node)
        {
            //if (node is Viewport viewport && viewport.IsInGroup(Consts.Groups.WorldViewport))
            //{
            //    SetViewportProperties(viewport);
            //}
        }

        private void SetAllViewportsProperties()
        {
            //foreach (var viewport in GetTree().GetNodesInGroup(Consts.Groups.WorldViewport).OfType<Viewport>())
            //{
            //    SetViewportProperties(viewport);
            //}
        }

        private void SetViewportProperties(Viewport viewport)
        {
            var resolution = _GameSettings.GetResolution();
            var scaling = _GameSettings.GetResolutionScaling();

            //// Set viewport size as scaled size
            //var scaledSize = (resolution * (scaling / 100f)).Round();
            //viewport.Size = scaledSize;

            //// Calculate and set sharpness
            //var screenWidth = resolution.x;
            //var d = screenWidth - viewport.Size.x;
            //var sharpness = Mathf.Clamp(d / screenWidth, 0, 1);
            //viewport.SharpenIntensity = sharpness;

            //// Set msaa and fxaa
            //var msaa = _GameSettings.GetMsaa();
            //var fxaa = _GameSettings.GetFxaa();
            //viewport.Msaa = (Viewport.MSAA)msaa;
            //viewport.Fxaa = fxaa;
        }

        public static Vector2 GetProjectViewportSize()
        {
            var width = ProjectSettings.GetSetting("display/window/size/width");
            var height = ProjectSettings.GetSetting("display/window/size/height");

            return new Vector2(
                int.Parse(width.ToString()),
                int.Parse(height.ToString())
            );
        }

        /*
         * If this is a built version of the game (debug or not)
         */
        public static bool IsRelease()
        {
            // Basically if we're running a debug build
            return OS.HasFeature("standalone");
        }

        /*
         * If the game is running in the editor, or is a debug build
         */
        public static bool IsDebugMode()
        {
            return OS.IsDebugBuild();
        }
    }
}