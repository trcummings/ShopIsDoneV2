using Godot;
using ShopIsDone.Utils.UI;
using System;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.Conditions.UI
{
    public partial class ConditionsUI : Control
    {
        [Export]
        public NodePath AllTasksDoneTweenerPath;

        [Export]
        public PackedScene ElementTemplate;

        // Nodes
        private Control _ConditionElementsNode;
        private ControlTweener _AllTasksDoneTweener;

        // State
        private Array<ConditionUIElement> _ConditionElements = new Array<ConditionUIElement>();

        public override void _Ready()
        {
            // Ready nodes
            _ConditionElementsNode = GetNode<Control>("%Conditions");
            _AllTasksDoneTweener = GetNode<ControlTweener>(AllTasksDoneTweenerPath);
        }

        public void Init(Array<Condition> conditions)
        {
            // Clear away remaining elements
            if (_ConditionElements.Count > 0)
            {
                foreach (var elem in _ConditionElements)
                {
                    elem.Hide();
                    _ConditionElementsNode.RemoveChild(elem);
                    elem.QueueFree();
                }
                _ConditionElements.Clear();
            }

            // Create a list of condition elements
            foreach (var condition in conditions.Where(c => c.IsVisibleInUI))
            {
                var elem = ElementTemplate.Instantiate<ConditionUIElement>();
                _ConditionElementsNode.AddChild(elem);
                _ConditionElements.Add(elem);
                elem.SetCondition(condition);
            }
        }

        public async void OnAllConditionsComplete()
        {
            await _AllTasksDoneTweener.TweenInAsync(0.5f);
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            await _AllTasksDoneTweener.TweenOutAsync(0.5f);
        }

        public void UpdateCondition(Condition condition)
        {
            var elem = _ConditionElements.ToList().Find(e => e.HasCondition(condition));
            elem?.UpdateCondition();
        }

        public void UpdateConditionUI()
        {
            foreach (var elem in _ConditionElements) elem.UpdateCondition();
        }
    }
}
