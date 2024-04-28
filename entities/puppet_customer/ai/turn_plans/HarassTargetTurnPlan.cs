using System;
using Godot.Collections;
using Godot;
using ShopIsDone.AI;

namespace ShopIsDone.Entities.PuppetCustomers.TurnPlans
{
    public partial class HarassTargetTurnPlan : TurnPlan
    {
        // Only valid if we haven't yet harassed an employee
        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            return !DidHarassEmployee();
        }

        public override int GetPriority()
        {
            // If we've harassed an employee already, then this is the highest priority
            // plan
            return DidHarassEmployee() ? int.MinValue : int.MaxValue;
        }

        private bool DidHarassEmployee()
        {
            return _Blackboard.ContainsKey(Consts.BOTHERED_EMPLOYEE) && (bool)_Blackboard[Consts.BOTHERED_EMPLOYEE];
        }
    }
}

