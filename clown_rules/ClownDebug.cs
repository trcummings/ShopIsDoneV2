using Godot;
using ShopIsDone.Utils.Extensions;
using System;
using System.Linq;

namespace ShopIsDone.ClownRules
{
    public partial class ClownDebug : Control
    {
        [Export]
        private Label _GroupRage;

        [Export]
        private Control _IndividualRages;

        [Export]
        private Control _RulesBroken;

        private ClownRulesService _ClownRulesService;

        public void Init(ClownRulesService service)
        {
            _ClownRulesService = service;
        }

        public override void _Process(double delta)
        {
            if (_ClownRulesService == null) return;

            SetRage();
            //SetBrokenRules();
        }

        private void SetRage()
        {
            _GroupRage.Text = $"group rage: {_ClownRulesService.GroupRage}";

            // Fix up number of labels
            var iRageCount = _IndividualRages.GetChildCount();
            if (iRageCount != _ClownRulesService.UnitRage.Count)
            {
                _IndividualRages.RemoveChildrenOfType<Label>();
                for (int i = 0; i < _ClownRulesService.UnitRage.Count; i++)
                {
                    _IndividualRages.AddChild(new Label());
                }
            }

            // Update each rage label
            var ids = _ClownRulesService.UnitRage.Keys.ToList();
            for (int j = 0; j < iRageCount; j++)
            {
                var label = _IndividualRages.GetChild(j) as Label;
                var id = ids[j];
                label.Text = $"{id} rage: {_ClownRulesService.UnitRage[id]}";
            }
        }

        //private void SetBrokenRules()
        //{
        //    // Fix up number of labels
        //    var brokenRulesCount = _RulesBroken.GetChildCount();
        //    if (brokenRulesCount != _ClownRulesService.BrokenRules.Count)
        //    {
        //        _RulesBroken.RemoveChildrenOfType<Label>();
        //        for (int i = 0; i < _ClownRulesService.BrokenRules.Count; i++)
        //        {
        //            _RulesBroken.AddChild(new Label());
        //        }
        //    }

        //    // Update each rage label
        //    var brokenRules = _ClownRulesService.BrokenRules.Keys.ToList();
        //    for (int j = 0; j < brokenRulesCount; j++)
        //    {
        //        var label = _RulesBroken.GetChild(j) as Label;
        //        label.Text = $"rule broken: {brokenRules[j].Name}";
        //    }
        //}
    }
}
