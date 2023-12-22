using Godot;
using System;

namespace ShopIsDone.Pausing
{
    public partial class PauseInputHandler : Node
    {
        [Signal]
        public delegate void GamePausedEventHandler();

        [Signal]
        public delegate void GameUnpausedEventHandler();

        [Export]
        public bool IsActive = false;

        public override void _Input(InputEvent @event)
        {
            if (IsActive && Input.IsActionJustPressed("pause"))
            {
                if (IsGamePaused()) RequestUnpause();
                else RequestPause();
            }
        }

        public void RequestPause()
        {
            PauseGame();
            EmitSignal(nameof(GamePaused));
        }

        public void RequestUnpause()
        {
            UnpauseGame();
            EmitSignal(nameof(GameUnpaused));
        }

        public bool IsGamePaused()
        {
            return GetTree().Paused;
        }

        private void PauseGame()
        {
            GetTree().Paused = true;
        }

        private void UnpauseGame()
        {
            GetTree().Paused = false;
        }
    }
}