using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;

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

        public override void OnStart(Dictionary<string, Variant> message)
        {
            // First connect to cutscene finishing
            _CutsceneService.Connect(
                nameof(_CutsceneService.CutsceneFinished),
                Callable.From(OnCutsceneFinished),
                (uint)ConnectFlags.OneShot
            );

            // Disable pausing
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
