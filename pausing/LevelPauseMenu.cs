using Godot;
using System;
using System.Linq;
using ShopIsDone.Game;
using ShopIsDone.UI;
using ShopIsDone.GameSettings;

namespace ShopIsDone.Pausing
{
    public partial class LevelPauseMenu : FocusMenu
    {
        [Signal]
        public delegate void UnpauseRequestedEventHandler();

        [Signal]
        public delegate void SuspendGameRequestedEventHandler();

        [Signal]
        public delegate void RestartLevelFromLoadEventHandler();

        [Signal]
        public delegate void QuitWithoutSaveRequestedEventHandler();

        [Export]
        public BlurBackground _BlurBackground;

        // Focus containers
        private Control _ButtonContainer;
        private SettingsMenu _SettingsMenu;

        // Menu Buttons
        private Button _ResumeGameButton;
        private Button _OpenLevelInEditorButton;
        private Button _EditLevelInProgressButton;
        private Button _RestartLevelFromLoadButton;
        private Button _ToLevelEditorButton;
        private Button _SettingsButton;
        private Button _SuspendGameButton;
        private Button _QuitWithoutSavingButton;

        private GameSettingsManager _GameSettings;

        public override void _Ready()
        {
            // Game settings
            _GameSettings = GameSettingsManager.GetGameSettings(this);
            _GameSettings.BlurDuringPauseChanged += OnBlurDuringPauseChanged;

            // Settings menu
            _SettingsMenu = GetNode<SettingsMenu>("%SettingsMenu");
            // Connect to events
            _SettingsMenu.ChangedTab += OnSettingsPressed;
            _SettingsMenu.CloseRequested += OnBackRequested;
            // Initially stop it from processing
            _SettingsMenu.SetProcess(false);

            // Editor Buttons
            _ButtonContainer = GetNode<Control>("%ButtonContainer");

            _ResumeGameButton = GetNode<Button>("%ResumeGameButton");
            _ResumeGameButton.Pressed += () => EmitSignal(nameof(UnpauseRequested));

            _RestartLevelFromLoadButton = GetNode<Button>("%RestartLevelFromLoadButton");
            _RestartLevelFromLoadButton.Pressed += () =>
            {
                // Do nothing for now
            };

            _SettingsButton = GetNode<Button>("%SettingsButton");
            _SettingsButton.Pressed += OnSettingsPressed;

            _SuspendGameButton = GetNode<Button>("%SuspendGameButton");
            _SuspendGameButton.Pressed += () =>
            {
                // Do nothing for now
            };

            _QuitWithoutSavingButton = GetNode<Button>("%QuitWithoutSavingButton");
            _QuitWithoutSavingButton.Pressed += () =>
            {
                var globalEvents = Events.GetEvents(this);
                globalEvents.EmitSignal(nameof(globalEvents.QuitGameRequested));
            };

            // Show or hide the blur background (only hide in debug mode)
            _BlurBackground.Visible = GameManager.IsDebugMode()
                ? _GameSettings.GetBlurDuringPause()
                : true;

            // Initialize settings menu values
            _SettingsMenu.InitializeValues();

            // Set focus container to button container
            SetFocusContainer(_ButtonContainer);
        }

        public void Activate()
        {
            Show();
            // Set focus on current focus container after visible
            SetFocusContainer(_FocusContainer);
            // Fade in
            _BlurBackground.FadeIn();
            SetProcess(true);
        }

        public void Deactivate()
        {
            Hide();
            // If settings menu is visible, revert to base menu
            RevertToBaseMenu();
            // Fade out
            _BlurBackground.FadeOut();
            SetProcess(false);
        }

        protected override void OnBackRequested()
        {
            // If settings menu is hidden, then just unpause
            if (!_SettingsMenu.Visible)
            {
                EmitSignal(nameof(UnpauseRequested));
            }
            // Otherwise, revert to base menu from settings menu
            else
            {
                RevertToBaseMenu();
            }
        }

        private void RevertToBaseMenu()
        {
            // Close the settings menu and open the base menu
            _ButtonContainer.Show();
            _SettingsMenu.Hide();

            // Stop the settings menu from processing
            _SettingsMenu.SetProcess(false);

            // Save the settings
            _SettingsMenu.SaveSettings();

            // Set focus container to button container
            SetFocusContainer(_ButtonContainer);
        }

        private void OnSettingsPressed()
        {
            // Hide the main menu and open the settings menu
            _ButtonContainer.Hide();
            _SettingsMenu.Show();

            // Allow the settings menu to process
            _SettingsMenu.SetProcess(true);

            // Set the focus container to the tab container
            SetFocusContainer(_SettingsMenu.GetFocusContainer());
        }

        private void OnBlurDuringPauseChanged(bool newValue)
        {
            // Only hide in debug mode
            _BlurBackground.Visible = GameManager.IsDebugMode()
                ? newValue
                : true;
        }
    }
}