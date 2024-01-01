using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models;
using Godot.Collections;

namespace ShopIsDone.EntityStates
{
    public partial class DoTaskEntityState : EntityState
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
            Task _ = _Model.PerformAnimation("do_task");
        }

        private void OnAnimationFinished(string animName)
        {
            if (_Model.TransformAnimName(animName) == "do_task") base.Enter();
        }

        public override bool IsInArena()
        {
            return true;
        }

        public override bool CanAct()
        {
            return true;
        }
    }
}

