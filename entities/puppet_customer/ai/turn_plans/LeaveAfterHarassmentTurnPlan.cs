using System;
using Godot.Collections;
using Godot;
using ShopIsDone.AI;

namespace ShopIsDone.Entities.PuppetCustomers.TurnPlans
{
    public partial class LeaveAfterHarassmentTurnPlan : TurnPlan
    {
        // Leave after harassment plan is only valid after we've harassed an employee
        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            return DidHarassEmployee();
        }

        public override int GetPriority()
        {
            // If we've harassed an employee already, then this is the highest priority
            // plan
            return DidHarassEmployee() ? int.MaxValue : int.MinValue;
        }

        private bool DidHarassEmployee()
        {
            return _Blackboard.ContainsKey(Consts.BOTHERED_EMPLOYEE) && (bool)_Blackboard[Consts.BOTHERED_EMPLOYEE];
        }
    }
}

