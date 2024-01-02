using Godot;
using System;
using ShopIsDone.Utils.StateMachine;
using Godot.Collections;
using ShopIsDone.Pausing;
using ShopIsDone.Utils.DependencyInjection;

namespace ShopIsDone.Arenas.States
{
	public partial class InitializingState : State
	{
        [Inject]
        private PauseInputHandler _PauseInputHandler;

        public override void OnStart(Dictionary<string, Variant> message = null)
        {
            InjectionProvider.Inject(this);

            // Disable Pausing
            _PauseInputHandler.IsActive = false;

            base.OnStart(message);

            // Go directly to running state
            ChangeState(Consts.RUNNING);
        }
    }
}
