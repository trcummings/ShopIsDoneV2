using System;
using Godot;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using Godot.Collections;
using System.Linq;
using ShopIsDone.Utils;
using ShopIsDone.Conditions.UI;

namespace ShopIsDone.Conditions
{
    public partial class ConditionsService : Node, IService, IInitializable
    {
        private Array<Condition> _Conditions = new Array<Condition>();

        [Export]
        private ConditionsUI _ConditionsRemaining;

        public void Init()
        {
            this.RecurseChildrenOfType<Condition>((_, condition) => InjectionProvider.Inject(condition));
            _Conditions = GetChildren().OfType<Condition>().ToGodotArray();

            // Init UI
            _ConditionsRemaining.Init(_Conditions);
        }

        public bool AllConditionsComplete()
        {
            return _Conditions.All(c => c.IsComplete());
        }
    }
}
