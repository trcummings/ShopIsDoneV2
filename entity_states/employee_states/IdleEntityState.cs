using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models.Employees;

namespace ShopIsDone.EntityStates.EmployeeStates
{
    public partial class IdleEntityState : EntityState
	{
		[Export]
		public EmployeeIsometricModel _Model;

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

