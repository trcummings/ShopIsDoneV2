using Godot;
using ShopIsDone.Conditions;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using System;
using StateConsts = ShopIsDone.EntityStates.Consts;

namespace ShopIsDone.Levels.TestLevel.Conditions
{
    public partial class CustomerSatisfiedCondition : Condition
    {
        [Export]
        private LevelEntity Customer;

        public override bool IsComplete()
        {
            return Customer.GetComponent<EntityStateHandler>().IsInState(StateConsts.DEAD);
        }
    }
}