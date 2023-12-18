using Godot;
using System.Linq;

namespace Microgames.DestroyRottenProduce
{
    public partial class ReticuleFist : Node2D
    {
        [Signal]
        public delegate void HitProduceEventHandler(BaseProduceItem produce);

        [Signal]
        public delegate void WhiffedEventHandler();

        [Export]
        private Area2D _ProduceDetector;

        [Export]
        private Node2D _FistPivot;

        [Export]
        private Sprite2D _FistSprite;

        public bool IsLaunching { get { return _IsLaunching; } }
        private bool _IsLaunching = false;

        public async void LaunchFist()
        {
            // Set is launching
            _IsLaunching = true;

            var launchTween = GetTree()
                .CreateTween()
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Bounce);

            launchTween
                .TweenProperty(_FistPivot, "position", new Vector2(0, 0), 0.1f);
            // Fade fist in
            launchTween
                .Parallel()
                .TweenProperty(_FistSprite, "modulate:a", 1, 0.1f);

            // Wait until finished
            await ToSignal(launchTween, "finished");

            // Check if we hit any produce, and emit
            var produceItems = _ProduceDetector
                .GetOverlappingBodies()
                .ToList()
                .Where(o => o is BaseProduceItem)
                .Select(o => o as BaseProduceItem);
            if (produceItems.Count() > 0)
            {
                // Pick the closest produce item
                var closestItem = produceItems
                    .OrderBy(p => p.GlobalPosition.DistanceTo(GlobalPosition))
                    .First();
                EmitSignal(nameof(HitProduce), closestItem);
            }
            // Otherwise whiff
            else EmitSignal(nameof(Whiffed));

            // Retract fist
            var retractTween = GetTree()
                .CreateTween()
                .SetEase(Tween.EaseType.OutIn)
                .SetTrans(Tween.TransitionType.Bounce);

            // Retract
            retractTween
                .TweenProperty(_FistPivot, "position", new Vector2(0, 400), 0.1f);
            // Fade back out
            retractTween
                .Parallel()
                .TweenProperty(_FistSprite, "modulate:a", 0.7, 0.1f);

            // Stop launching after half of the retraction
            await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
            _IsLaunching = false;
        }
    }
}

