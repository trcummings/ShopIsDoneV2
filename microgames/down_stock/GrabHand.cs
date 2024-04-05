using System;
using Godot;

namespace ShopIsDone.Microgames.DownStock
{
	public partial class GrabHand : CharacterBody2D
	{
		private Sprite2D _Sprite;

        public override void _Ready()
        {
            base._Ready();
			_Sprite = GetNode<Sprite2D>("%Sprite2D");
        }

		public bool IsGrabbing()
		{
			return _Sprite.Frame == 1;
		}

		public void Grab()
		{
			_Sprite.Frame = 1;
		}

		public void Release()
		{
            _Sprite.Frame = 0;
        }
	}
}

