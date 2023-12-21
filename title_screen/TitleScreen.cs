using Godot;
using ShopIsDone.GameSettings;
using System;
using System.Linq;

namespace ShopIsDone.TitleScreen
{
    public partial class TitleScreen : FocusMenu
    {
        [Signal]
        public delegate void LevelEditorRequestedEventHandler();

        [Signal]
        public delegate void ContinueRequestedEventHandler();

        [Signal]
        public delegate void QuitGameRequestedEventHandler();

        // Nodes
        private Control _ButtonContainer;
        private Button _LevelEditorButton;
        private Button _ContinueButton;
        private Button _NewGameButton;
        private Button _SettingsButton;
        private Button _QuitGameButton;

        // Settings menu
        private SettingsMenu _SettingsMenu;

        public override void _Ready()
        {
            // Ready nodes
            _ButtonContainer = GetNode<Control>("%ButtonContainer");
            _LevelEditorButton = GetNode<Button>("%LevelEditorButton");
            _ContinueButton = GetNode<Button>("%ContinueButton");
            _NewGameButton = GetNode<Button>("%NewGameButton");
            _SettingsButton = GetNode<Button>("%SettingsButton");
            _QuitGameButton = GetNode<Button>("%QuitGameButton");

            // Connect to button events
            _LevelEditorButton.Connect("pressed", new Callable(this, nameof(OnLevelEditorPressed)));
            _ContinueButton.Connect("pressed", new Callable(this, nameof(OnContinuePressed)));
            _QuitGameButton.Connect("pressed", new Callable(this, nameof(OnQuitGamePressed)));
            _SettingsButton.Connect("pressed", new Callable(this, nameof(OnSettingsPressed)));

            // TODO: If we have a Suspended Game folder, then we can continue


            // Ready settings menu
            _SettingsMenu = GetNode<SettingsMenu>("%SettingsMenu");
            // Connect to events
            _SettingsMenu.ChangedTab += OnSettingsPressed;
            _SettingsMenu.CloseRequested += OnBackRequested;
            // Initially stop it from processing
            _SettingsMenu.SetProcess(false);

            // Set focus container
            SetFocusContainer(_ButtonContainer);
        }

        public void Init()
        {
            _SettingsMenu.InitializeValues();
        }

        private void OnLevelEditorPressed()
        {
            EmitSignal(nameof(LevelEditorRequested));
        }

        private void OnContinuePressed()
        {
            EmitSignal(nameof(ContinueRequested));
        }

        private void OnQuitGamePressed()
        {
            EmitSignal(nameof(QuitGameRequested));
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

        protected override void OnBackRequested()
        {
            // Close the settings menu and open the main menu
            _ButtonContainer.Show();
            _SettingsMenu.Hide();

            // Stop the settings menu from processing
            _SettingsMenu.SetProcess(false);

            // Save settings
            _SettingsMenu.SaveSettings();

            // Set focus container to button container
            SetFocusContainer(_ButtonContainer);
        }
    }
}