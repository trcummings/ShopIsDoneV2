using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;

namespace ShopIsDone.Levels.States
{
	public partial class GameOverState : State
	{
        [Export]
        private PauseInputHandler _PauseInputHandler;

        [Export]
        private GameOverUI _GameOverUI;

        public override void _Ready()
        {
            base._Ready();
            _GameOverUI.Deactivate();
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Disable pausing
            _PauseInputHandler.IsActive = false;

            // Activate Game over UI
            // TODO: Connect to game over events
            _GameOverUI.Activate();
        }

        public override void OnExit(string nextState)
        {
            // Deactivate Game over UI
            // TODO: Disconnect from game over events
            _GameOverUI.Deactivate();

            base.OnExit(nextState);
        }
    }
}