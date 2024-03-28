using Godot;
using System;

namespace ShopIsDone.Microgames.FindBathroom
{
	public partial class CartSpawner : Node2D
	{
		[Export]
		private Node2D _LeftPos;

        [Export]
        private Node2D _RightPos;

		[Export]
		public Directions Direction = Directions.ToLeft;
        public enum Directions
        {
            ToLeft,
            ToRight
        }

        [Export]
		private PackedScene _CartScene;

		private Tween _CartTween;

		public void Start()
		{
			var cart = _CartScene.Instantiate<Area2D>();
			AddChild(cart);

			// Position it and start it off
			_CartTween = GetTree().CreateTween().BindNode(this);
			if (Direction == Directions.ToLeft)
			{
				cart.GlobalPosition = _RightPos.GlobalPosition;
				_CartTween.TweenProperty(cart, "global_position", _LeftPos.GlobalPosition, 2f);
				_CartTween.TweenCallback(new Callable(cart, "queue_free"));
			}
			else
			{
                cart.GlobalPosition = _LeftPos.GlobalPosition;
				// Flip the scale
				cart.Scale = new Vector2(-1, 1);
                _CartTween.TweenProperty(cart, "global_position", _RightPos.GlobalPosition, 2f);
                _CartTween.TweenCallback(new Callable(cart, "queue_free"));
            }
		}

		public void Stop()
		{
			_CartTween?.Kill();
			_CartTween = null;
        }
	}
}
