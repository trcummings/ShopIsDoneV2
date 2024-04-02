using System;
using Godot;

namespace ShopIsDone.Microgames.SaladBar
{
    public partial class Shambler : BaseCustomer
	{
        private Sprite2D _ShadeFog;

        public override void _Ready()
        {
            base._Ready();
            _ShadeFog = GetNode<Sprite2D>("%ShadeFog");
        }

        public override void Leave()
        {
            GetTree()
                .CreateTween()
                .BindNode(this)
                .TweenProperty(_ShadeFog, "modulate:a", 0, 1f);
            base.Leave();
        }
    }
}

