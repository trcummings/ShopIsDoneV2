using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;

namespace ShopIsDone.Levels.States
{
    public partial class ExitingState : State
    {
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Disable pausing
            _PauseInputHandler.IsActive = false;

            // Idle the player characters
            GD.Print("Idling!");
            _PlayerCharacterManager.Idle();

            // TODO: Pause any other actors

            base.OnStart(message);
        }
    }
}

