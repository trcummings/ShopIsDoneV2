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

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            // Disable pausing
            _PauseInputHandler.IsActive = false;

            // TODO: Active Game Over screen
        }
    }
}