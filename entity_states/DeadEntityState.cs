using Godot;
using Godot.Collections;
using ShopIsDone.Models;
using System;
using System.Threading.Tasks;

namespace ShopIsDone.EntityStates
{
	public partial class DeadEntityState : EntityState
    {
        [Export]
        public NodePath ModelPath;
        private IModel _Model;

        private Callable _AnimFinished;

        public override void _Ready()
        {
            base._Ready();
            _Model = GetNode<IModel>(ModelPath);
            _AnimFinished = new Callable(this, nameof(OnAnimationFinished));
        }

        public override void Enter(Dictionary<string, Variant> message = null)
        {
            (_Model as Node).Connect(nameof(Model.AnimationFinished), _AnimFinished, (uint)ConnectFlags.OneShot);
            Task _ = _Model.PerformAnimation("die");
        }

        private void OnAnimationFinished(string animName)
        {
            if (_Model.TransformAnimName(animName) == "die") base.Enter();
        }

        public override bool IsInArena()
        {
            return true;
        }

        public override bool CanAct()
        {
            return false;
        }
    }
}
