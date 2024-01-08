using System;
using Godot;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.Conditions
{
    public partial class ConditionsService : Node, IService
    {
        private Array<Condition> _Conditions = new Array<Condition>();

        public void Init()
        {
            this.RecurseChildrenOfType<Condition>((_, condition) => InjectionProvider.Inject(condition));
            _Conditions = GetChildren().OfType<Condition>().ToGodotArray();
        }

        public bool AllConditionsComplete()
        {
            return _Conditions.All(c => c.IsComplete());
        }
    }
}
