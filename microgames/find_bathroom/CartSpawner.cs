using Godot;
using ShopIsDone.Utils;
using System;
using Godot.Collections;

namespace ShopIsDone.Microgames.FindBathroom
{
	public partial class CartSpawner : Node2D, IStoppable
    {
		[Export]
		public Directions Direction = Directions.ToLeft;
        public enum Directions
        {
            ToLeft,
            ToRight
        }

        [Export]
		private PackedScene _CartScene;

        [Export]
        private float _CartTimeToCross = 2f;

        private Node2D _LeftPos;
        private Node2D _RightPos;
        private Timer _Timer;

        private Array<CartState> _Carts = new Array<CartState>();
        private partial class CartState : GodotObject
        {
            public Tween Tween;
            public Area2D Cart;
        }

        public override void _Ready()
        {
            base._Ready();
            _LeftPos = GetNode<Node2D>("%LeftPos");
            _RightPos = GetNode<Node2D>("%RightPos");
            _Timer = GetNode<Timer>("%Timer");
        }

        public void Start(float betweenCarts)
		{
            SpawnCart();

            // Connect to the timer event and let it repeat
            _Timer.OneShot = false;
            _Timer.WaitTime = betweenCarts;
            _Timer.Timeout += SpawnCart;
            _Timer.Start();
		}

		private void SpawnCart()
		{
            var cart = _CartScene.Instantiate<Area2D>();
            AddChild(cart);
            var cartState = new CartState()
            {
                Cart = cart,
                Tween = GetTree().CreateTween().BindNode(this)
            };
            _Carts.Add(cartState);

            // Position it and start it off
            if (Direction == Directions.ToLeft)
            {
                cart.GlobalPosition = _RightPos.GlobalPosition;
                cartState.Tween.TweenProperty(cart, "global_position", _LeftPos.GlobalPosition, _CartTimeToCross);
            }
            else
            {
                cart.GlobalPosition = _LeftPos.GlobalPosition;
                // Flip the scale
                cart.Scale = new Vector2(-1, 1);
                cartState.Tween.TweenProperty(cart, "global_position", _RightPos.GlobalPosition, _CartTimeToCross);
            }
            // On finish tween callback
            cartState.Tween.TweenCallback(Callable.From(() => RemoveCart(cartState)));
        }

        private void RemoveCart(CartState cartState)
        {
            cartState.Cart.QueueFree();
            _Carts.Remove(cartState);
        }

		public void Stop()
		{
            // Stop timer
            _Timer.Stop();

            // Stop all carts
            foreach (var cartState in _Carts)
            {
                cartState.Tween?.Kill();
                cartState.Tween = null;
                cartState.Cart.GetNode<AnimatedSprite2D>("Customer").Stop();
                cartState.Cart.GetNode<AnimatedSprite2D>("Cart").Stop();
            }
        }
	}
}
