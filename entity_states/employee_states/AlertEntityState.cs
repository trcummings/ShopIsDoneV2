using System;
using Godot;
using ShopIsDone.Models.Employees;

namespace ShopIsDone.EntityStates.EmployeeStates
{
    public partial class AlertEntityState : EntityState
    {
        [Export]
        public EmployeeIsometricModel _Model;

        public async override void Enter()
        {
            await _Model.PerformAnimation("alert");
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