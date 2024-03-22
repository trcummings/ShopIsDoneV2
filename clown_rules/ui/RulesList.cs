using Godot;
using System;
using Godot.Collections;
using System.Threading.Tasks;
using System.Linq;
using ShopIsDone.Utils.UI;
using ShopIsDone.ClownRules.ActionRules;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.ClownRules.UI
{
    public partial class RulesList : Control
    {
        [Export]
        public PackedScene RuleElementScene;

        // Nodes
        private Control _Rules;
        private ControlTweener _ControlTweener;
        private TextureRect _RulesBoard;

        // State
        private RandomNumberGenerator _RNG = new RandomNumberGenerator();

        public override void _Ready()
        {
            // Ready nodes
            _Rules = GetNode<Control>("%Rules");
            _ControlTweener = GetNode<ControlTweener>("%ControlTweener");
            _RulesBoard = GetNode<TextureRect>("%RulesBoard");

            _RNG.Randomize();
        }

        public void Init(Array<ClownActionRule> rules)
        {
            // Clear away any existing rules
            _Rules.RemoveChildrenOfType<RichTextLabel>();

            // Create a list of condition elements
            foreach (var rule in rules)
            {
                var elem = RuleElementScene.Instantiate<RichTextLabel>();
                _Rules.AddChild(elem);
                elem.Text = rule.RuleDescription;
                elem.SetMeta("rule", rule);
            }
        }

        public void HighlightRule(ClownActionRule rule)
        {
            // Find rule with given ID
            var ruleNode = _Rules
                .GetChildren()
                .OfType<RichTextLabel>()
                .Where(e => rule == (ClownActionRule)e.GetMeta("rule"))
                .First();

            // Ignore if no found rule
            if (rule == null) return;

            // Set rule values
            ruleNode.Text = $"[color=red][nervous scale={2f} freq={15f}]{rule.RuleDescription}[/nervous][/color]";

            //// Tween board shake
            //var prevPos = ruleNode.RectPosition;
            //var tween = CreateTween().SetLoops(20).SetTrans(Tween.TransitionType.Bounce);
            //tween.TweenProperty(ruleNode, "rect_position", prevPos + new Vector2(2, 0), 0.025f);
            //tween.TweenProperty(ruleNode, "rect_position", prevPos, 0.025f);
            //tween.TweenProperty(ruleNode, "rect_position", prevPos + new Vector2(-2, 0), 0.025f);
            //tween.TweenProperty(ruleNode, "rect_position", prevPos, 0.025f);
        }

        public Task TweenIn(float duration)
        {
            // Set rules board pivot to halfway on x, 1/4 on y
            _RulesBoard.PivotOffset = new Vector2(
                _RulesBoard.Size.X / 2f,
                _RulesBoard.Size.Y / 4f
            );

            // Pick random direction
            var sign = Mathf.Sign(_RNG.RandiRange(-2, 2));
            sign = sign == 0 ? -1 : sign;

            var tween = CreateTween().BindNode(this);
            // Rotate forward
            tween
                .TweenProperty(_RulesBoard, "rect_rotation", sign * 7f, duration / 2f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);

            // Rotate back (a little too much)
            tween
                .Chain()
                .TweenProperty(_RulesBoard, "rect_rotation", sign * (-1f), duration / 2f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);

            // Rotate back
            tween
                .Chain()
                .TweenProperty(_RulesBoard, "rect_rotation", 0f, duration / 4f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Bounce);

            // Tween in vertically
            return _ControlTweener.TweenInAsync(duration);
        }

        public async Task TweenOut(float duration)
        {
            await _ControlTweener.TweenInAsync(duration);

            // Reset rules
            foreach (var ruleNode in _Rules.GetChildren().OfType<RichTextLabel>())
            {
                var rule = (ClownActionRule)ruleNode.GetMeta("rule");
                ruleNode.Text = rule.RuleDescription;
            }

            // Reset board position
            _RulesBoard.Position = Vector2.Zero;
        }
    }
}