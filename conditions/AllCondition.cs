using System;
using System.Linq;

namespace ShopIsDone.Conditions
{
	public partial class AllCondition : ComposedCondition
	{
        public override bool IsComplete()
        {
            return _Conditions.All(condition => condition.IsComplete());
        }
    }
}

