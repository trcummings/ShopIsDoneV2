using System;
using System.Linq;
using Godot;

namespace ShopIsDone.Conditions
{
    public partial class SomeOfCondition : ComposedCondition
    {
        [Export]
        public int NumRequired = 1;

        public override bool IsComplete()
        {
            return _Conditions
                // Collect completed condition count
                .Aggregate(0, (acc, condition) =>
                    condition.IsComplete()
                        ? acc + 1
                        : acc
                // Weight against required number
                ) >= NumRequired;
        }
    }
}

