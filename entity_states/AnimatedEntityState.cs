using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace ShopIsDone.EntityStates
{
	public partial class AnimatedEntityState : EntityState
	{
        [Export]
        private bool _WaitForEnterAnimToFinish = true;

        [Export]
        private string _EnterAnimName;

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            _ = RunEnterAnimation();
        }

        protected async Task RunEnterAnimation()
        {
            // If we weren't given an enter animation name, just end immediately
            if (string.IsNullOrEmpty(_EnterAnimName))
            {
                base.Enter();
                return;
            }

            // If we wait for it to finish, call base enter afterwards to end
            else if (_WaitForEnterAnimToFinish)
            {
                await _ModelComponent.PerformActionAsync(_EnterAnimName);
                base.Enter();
                return;
            }

            // Otherwise run the enter animation and end immediately
            _ = _ModelComponent.PerformActionAsync(_EnterAnimName);
            base.Enter();
        }
    }
}

