using Godot;
using ShopIsDone.UI;
using System;

namespace ShopIsDone.Levels
{
    public partial class GameOverUI : FocusMenu
    {
        [Signal]
        public delegate void RequestedRestartFromCheckpointEventHandler();

        [Signal]
        public delegate void RequestedRestartLevelEventHandler();

        [Signal]
        public delegate void RequestedQuitToTitleEventHandler();

        [Signal]
        public delegate void RequestedQuitGameEventHandler();

        [Export]
        private Button _RestartFromCheckpoint;

        [Export]
        private Button _RestartLevel;

        [Export]
        private Button _QuitToTitle;

        [Export]
        private Button _QuitGame;

        [Export]
        private Control _Container;

        public override void _Ready()
        {
            SetFocusContainer(_Container);
            _RestartFromCheckpoint.Pressed += () => EmitSignal(nameof(RequestedRestartFromCheckpoint));
            _RestartLevel.Pressed += () => EmitSignal(nameof(RequestedRestartLevel));
            _QuitToTitle.Pressed += () => EmitSignal(nameof(RequestedQuitToTitle));
            _QuitGame.Pressed += () => EmitSignal(nameof(RequestedQuitGame));
        }
    }
}

