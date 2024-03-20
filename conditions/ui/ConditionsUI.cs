using Godot;
using Godot.Collections;
using System.Linq;
using ShopIsDone.UI;

namespace ShopIsDone.Conditions.UI
{
    public partial class ConditionsUI : Control, IVerticalDrawerChild
    {
        [Export]
        public PackedScene ElementTemplate;

        // Nodes
        private Control _ConditionElementsNode;
        private Control _TitleContainer;
        public Control DrawerFace { get { return _TitleContainer; } }

        // State
        private Array<ConditionUIElement> _ConditionElements = new Array<ConditionUIElement>();

        public override void _Ready()
        {
            // Ready nodes
            _ConditionElementsNode = GetNode<Control>("%DrawerContents");
            _TitleContainer = GetNode<Control>("%TitleContainer");
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
