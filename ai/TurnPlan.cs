using System;
using Godot.Collections;
using Godot;

namespace ShopIsDone.AI
{
    public partial class TurnPlan : Resource
    {
        [Export]
        public Array<ActionPlan> ActionPlans;
    }
}

