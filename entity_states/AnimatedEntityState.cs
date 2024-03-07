using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace ShopIsDone.EntityStates
{
	public partial class AnimatedEntityState : EntityState
	{
        [ExportGroup("Enter")]
        [Export]
        private bool _WaitForEnterAnimToFinish = true;

        [Export]
        private string _EnterAnimName;

        [ExportGroup("Exit")]
        [Export]
        private bool _WaitForExitAnimToFinish = true;

        [Export]
        private string _ExitAnimName;

        [ExportGroup("")]
        [Export]
        private string _IdleAnimName;

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            _ = RunEnterAnimation();
        }

        public override void Idle()
        {
            if (!string.IsNullOrEmpty(_IdleAnimName))
            {
                _ = _ModelComponent.PerformActionAsync(_IdleAnimName);
            }
        }

        public override void Exit()
        {
            _ = RunExitAnimation();
        }

        protected async Task RunExitAnimation()
        {
            // If we weren't given an animation name, just end immediately
            if (string.IsNullOrEmpty(_ExitAnimName))
            {
                base.Exit();
                return;
            }
            // If we wait for it to finish, call base afterwards to end
            else if (_WaitForExitAnimToFinish)
            {
                await _ModelComponent.PerformActionAsync(_EnterAnimName);
                base.Exit();
                return;
            }

            // Otherwise run the animation and end immediately
            _ = _ModelComponent.PerformActionAsync(_ExitAnimName);
            base.Exit();
        }

        protected async Task RunEnterAnimation()
        {
            // If we weren't given an animation name, just end immediately
            if (string.IsNullOrEmpty(_EnterAnimName))
            {
                base.Enter();
                return;
            }

            // If we wait for it to finish, call base afterwards to end
            else if (_WaitForEnterAnimToFinish)
            {
                await _ModelComponent.PerformActionAsync(_EnterAnimName);
                base.Enter();
                return;
            }

            // Otherwise run the animation and end immediately
            _ = _ModelComponent.PerformActionAsync(_EnterAnimName);
            base.Enter();
        }
    }
}

