using System;
using Godot;
using System.Threading.Tasks;
using ShopIsDone.Models.Employees;

namespace ShopIsDone.EntityStates.EmployeeStates
{
    public partial class MoveEntityState : EntityState
    {
        [Export]
        public EmployeeIsometricModel _Model;

        public override void Enter()
        {
            Task _ = _Model.PerformAnimation("walk");
            base.Enter();
        }
    }
}

