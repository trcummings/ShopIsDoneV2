using Godot;
using ShopIsDone.ClownRules.UI;
using ShopIsDone.Conditions.UI;
using ShopIsDone.UI;
using ShopIsDone.Utils;
using ShopIsDone.Utils.DependencyInjection;
using System;

namespace ShopIsDone.ClownRules
{
    public partial class RuleDisplayService : Node, IService, IInitializable, ICleanUpable
    {
        [Export]
        private VerticalDrawer _RulesContainer;

        [Export]
        private VerticalDrawer _ConditionsContainer;

        [Export]
        private RulesUI _RulesUI;

        [Export]
        private ConditionsUI _ConditionsUI;


        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);
        }

        public void Init()
        {
            _RulesContainer.InitDrawer();
            _ConditionsContainer.InitDrawer();
        }

        public void CleanUp()
        {
            _RulesContainer.HideDrawer();
            _ConditionsContainer.InitDrawer();
        }

        public override void _Process(double delta)
        {
            // Drop em down
            if (Input.IsActionJustPressed("show_conditions"))
            {
                _RulesContainer.PullOut();
                _ConditionsContainer.PullOut();
            }
            // If just released, pull them back up
            else if (Input.IsActionJustReleased("show_conditions"))
            {
                _RulesContainer.PushIn();
                _ConditionsContainer.PushIn();
            }
            // Handle released but still out
            else if (
                !Input.IsActionPressed("show_conditions") &&
                (_RulesContainer.IsExtended() || _ConditionsContainer.IsExtended())
            )
            {
                _RulesContainer.PushIn();
                _ConditionsContainer.PushIn();
            }
        }

        public void Activate()
        {
            SetProcess(true);
            _RulesContainer.Activate();
            _ConditionsContainer.Activate();
        }

        public void Deactivate()
        {
            SetProcess(false);
            _RulesContainer.Deactivate();
            _ConditionsContainer.Deactivate();
        }
    }
}

