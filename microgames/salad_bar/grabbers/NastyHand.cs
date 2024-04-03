using Godot;
using ShopIsDone.Levels;
using System;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class NastyHand : Grabber
    {
        private Sprite2D _Sprite;

        public override void _Ready()
        {
            base._Ready();
            _Sprite = GetNode<Sprite2D>("%Sprite");
            _Sprite.FlipH = LevelRngService.RandomBool();
        }
    }
}
