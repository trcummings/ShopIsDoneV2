using Godot;
using ShopIsDone.GameSettings;
using System;
using System.Linq;
using Godot.Collections;
using System.Threading.Tasks;
using ShopIsDone.Debug;
using ShopIsDone.Utils.StateMachine;

namespace ShopIsDone.Game
{
    public partial class GameManager : Node
    {
        [Export(PropertyHint.Enum)]
        public InitialGameStates OverrideModeAfterLoad = InitialGameStates.MainMenu;

        [Export]
        public string InitialLevelId;

        // Enum for what game mode should be picked
        public enum InitialGameStates
        {
            VanityCard = 0,
            MainMenu = 1,
            Level = 2
        }

        // Nodes
        private StateMachine _GSM;
        private ColorRect _BlackOverlay;
        private DebugDisplay _DebugDisplay;
        private GameSettingsManager _GameSettings;
        private Events _Events;

        public override void _Ready()
        {
            // Ready nodes
            _GSM = GetNode<StateMachine>("%GameStateMachine");
            _BlackOverlay = GetNode<ColorRect>("%BlackOverlay");
            _DebugDisplay = GetNode<DebugDisplay>("%DebugDisplay");

            // Show black overlay
            _BlackOverlay.Color = Colors.Black;
            _BlackOverlay.Show();

            // Game settings
            _GameSettings = GameSettingsManager.GetGameSettings(this);
            _GameSettings.ShowDebugDisplayChanged += SetDebugDisplayVisibility;

            // Events
            _Events = Events.GetEvents(this);
            _Events.FadeInRequested += () => _ = FadeInOverlay();
            _Events.FadeOutRequested += () => _ = FadeOutOverlay();
            _Events.QuitGameRequested += QuitGame;

            // Set GSM into settings load state
            _GSM.ChangeState(Consts.GameStates.INITIAL_LOAD, new Dictionary<string, Variant>()
            {
                { Consts.OVERRIDE_GAME_STATE, (int)OverrideModeAfterLoad },
                { Consts.INITIAL_LEVEL, InitialLevelId }
            });
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
        private void QuitGame()
        {
            // Save settings
            _GameSettings.Save();

            // Quit game
            GetTree().Quit();
        }

        // Debug Display
        private void SetDebugDisplayVisibility(bool value)
        {
            _DebugDisplay.SetVisibility(value);
        }

        // Overlay
        public async Task FadeOutOverlay()
        {
            var tween = GetTree().CreateTween().BindNode(this);
            tween.TweenProperty(_BlackOverlay, "color", new Color(0, 0, 0, 0), 0.5F);
            await ToSignal(tween, "finished");
            _Events.EmitSignal(nameof(_Events.FadeOutFinished));
        }

        public async Task FadeInOverlay()
        {
            var tween = GetTree().CreateTween().BindNode(this);
            tween.TweenProperty(_BlackOverlay, "color", Colors.Black, 0.5F);
            await ToSignal(tween, "finished");
            _Events.EmitSignal(nameof(_Events.FadeInFinished));
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
         * If this is a built version of the game (non-debug build
         */
        public static bool IsRelease()
        {
            // Basically if we're running a standalone build
            return OS.HasFeature("template");
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