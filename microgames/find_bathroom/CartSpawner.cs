using Godot;
using ShopIsDone.Utils;
using System;
using Godot.Collections;

namespace ShopIsDone.Microgames.FindBathroom
{
	public partial class CartSpawner : Node2D, IStoppable
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
		private Array<Area2D> _Carts = new Array<Area2D>();

		public void Start()
		{
			var cart = _CartScene.Instantiate<Area2D>();
			AddChild(cart);
			_Carts.Add(cart);

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
			foreach (var cart in _Carts)
			{
				cart.GetNode<AnimatedSprite2D>("Customer").Stop();
                cart.GetNode<AnimatedSprite2D>("Cart").Stop();
            }
        }
	}
}
