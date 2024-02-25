using Godot;
using ShopIsDone.Actions;
using System;
using Godot.Collections;

namespace ShopIsDone.ClownRules.ActionRules
{
    public partial class ClownActionRule : Node
    {
        // Cosmetics
        [Export(PropertyHint.MultilineText)]
        public string RuleDescription = "";

        public virtual bool BrokeRule(ArenaAction action, Dictionary<string, Variant> message)
        {
            return false;
        }

        public virtual void ResetRule()
        {

        }
    }
}

