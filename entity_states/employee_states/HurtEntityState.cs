using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Models.Employees;

namespace ShopIsDone.EntityStates.EmployeeStates
{
    public partial class HurtEntityState : EntityState
    {
        [Export]
        public EmployeeIsometricModel _Model;

        public override void Enter()
        {
            Task _ = _Model.PerformAnimation("hurt");
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

