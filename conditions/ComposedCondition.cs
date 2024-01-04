using System;
using System.Linq;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Conditions
{
    // This is a decorator over a list of conditions 
    public partial class ComposedCondition : Condition
    {
        protected Array<Condition> _Conditions = new Array<Condition>();

        public override void _Ready()
        {
            base._Ready();
            _Conditions = GetChildren().OfType<Condition>().ToGodotArray();
        }

        public override bool HasCondition(Condition condition)
        {
            return _Conditions.Contains(condition);
        }
    }
}

