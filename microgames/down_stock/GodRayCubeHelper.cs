using Godot;
using System;

namespace ShopIsDone.Microgames.DownStock
{
	public partial class GodRayCubeHelper : MeshInstance3D
	{
		public override void _Ready()
		{
            Position = Position with { Y = 0 };
            Scale = Scale with { Y = 0 };
            Hide();
        }

        public void Reveal()
        {
            Show();
            Position = Position with { Y = 0 };

            var tween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetParallel(true);

            tween.TweenProperty(this, "scale:y", 0.1f, 0.1f);
            tween.TweenProperty(this, "position:y", 0.1f, 0.1f);
        }

        public void Collapse()
        {
            var tween = GetTree()
                .CreateTween()
                .BindNode(this);

            tween.Parallel().TweenProperty(this, "scale:y", 0f, 0.1f);
            tween.Parallel().TweenProperty(this, "position:y", 0f, 0.1f);
            tween.TweenCallback(Callable.From(Hide));
        }
    }

}
