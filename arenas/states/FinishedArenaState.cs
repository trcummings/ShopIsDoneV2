using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using FinishedConsts = ShopIsDone.Arenas.States.Finished.Consts;
using ShopIsDone.Pausing;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.States
{
    public partial class FinishedArenaState : State
    {
        [Export]
        public StateMachine _OutcomeStateMachine;

        [Inject]
        private PauseInputHandler _PauseInputHandler;

        public override void OnStart(Dictionary<string, Variant> message)
        {
            base.OnStart(message);

            InjectionProvider.Inject(this);

            // Disable Pausing
            _PauseInputHandler.IsActive = false;

            // Get the type of finished state
            var state = (string)message?[FinishedConsts.FINISHED_STATE_KEY];
            _OutcomeStateMachine.ChangeState(state, message);
        }
    }
}
