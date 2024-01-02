using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models;
using Godot.Collections;

namespace ShopIsDone.EntityStates
{
	public partial class AnimatedEntityState : EntityState
	{
        [Export]
        private NodePath _ModelPath;
        protected IModel _Model;

        [Export]
        private bool _WaitForEnterAnimToFinish = true;

        [Export]
        private string _EnterAnimName;

        public override void _Ready()
        {
            base._Ready();
            _Model = GetNode<IModel>(_ModelPath);
        }

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            RunEnterAnimation();
        }

        protected void RunEnterAnimation()
        {
            // If we weren't given an enter animation name, just end immediately
            if (string.IsNullOrEmpty(_EnterAnimName))
            {
                base.Enter();
                return;
            }

            // Only connect to the enter animation if we're going to wait for it
            // to finish (as a one shot)
            if (_WaitForEnterAnimToFinish)
            {
                (_Model as Node)?.Connect(
                    nameof(Model.AnimationFinished),
                    new Callable(this, nameof(OnEnterAnimFinished)),
                    (uint)ConnectFlags.OneShot
                );
            }

            // Run the enter animation
            Task _ = _Model.PerformAnimation(_EnterAnimName);

            // If we're not waiting for this animation to finish, then just end
            // immediately
            if (!_WaitForEnterAnimToFinish) base.Enter();
        }

        private void OnEnterAnimFinished(string animName)
        {
            if (animName == _EnterAnimName) base.Enter();
        }
    }
}

