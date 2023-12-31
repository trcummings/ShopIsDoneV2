using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models;

namespace ShopIsDone.EntityStates
{
    public partial class IdleEntityState : EntityState
	{
		[Export]
		public NodePath ModelPath;
        private IModel _Model;

        public override void _Ready()
        {
            base._Ready();
            _Model = GetNode<IModel>(ModelPath);
        }

        public override void Enter()
        {
            Task _ = _Model.PerformAnimation("idle");
            base.Enter();
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

