using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Levels.States
{
    // For this state, we want to turn off every single type of actor that may
    // be currently doing anything
	public partial class CutsceneState : State
	{
        [Export]
        private PlayerCharacterManager _PlayerCharacterManager;

        [Export]
        private PauseInputHandler _PauseInputHandler;

        [Export]
        private CutsceneService _CutsceneService;

        public override void _Ready()
        {
            base._Ready();
            _CutsceneService.CutsceneFinished += OnCutsceneFinished;
        }

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // Enable pausing
            _PauseInputHandler.IsActive = true;

            // Idle the player characters
            _PlayerCharacterManager.Idle();

            // TODO: Pause any other actors

            // Finish the start hook
            base.OnStart(message);
        }

        private void OnCutsceneFinished()
        {
            // When cutscene finishes go back to previous state
            var (prevState, prevMessage) = GetLastStateProps();
            ChangeState(prevState, prevMessage);
        }
    }
}
