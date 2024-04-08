using Godot;
using System;

namespace ShopIsDone.Microgames.DownStock
{
    public partial class StockItem : Area3D, IHoverable
    {
        public int Id;
        private Node3D _Item;
        private Tween _WiggleTween;

        public override void _Ready()
        {
            base._Ready();
            _Item = GetNode<Node3D>("%Item");
        }

        public void Init(int id)
        {
            Id = id;
            RotateRandom();
        }

        public void RotateRandom()
        {
            // Add some mild random rotation to the stock items
            var randRotation = (float)GD.RandRange(-Mathf.Pi / 10, Mathf.Pi / 10);
            RotateY(randRotation);
        }

        public void Hover()
        {
            TweenScale(Vector3.One * 1.1f);
            Wiggle();
        }

        public void Unhover()
        {
            TweenScale(Vector3.One);
            StopWiggle();
        }

        public void Wiggle()
        {
            _WiggleTween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetLoops();
            _WiggleTween.TweenProperty(_Item, "position:x", 0.01f, .1f).AsRelative();
            _WiggleTween.TweenProperty(_Item, "position:x", -0.01f, .1f).AsRelative();
        }

        public void StopWiggle()
        {
            _WiggleTween?.Kill();
            _WiggleTween = null;
            _Item.Position = _Item.Position with { X = 0 };
        }

        private void TweenScale(Vector3 scale)
        {
            var tween = GetTree()
                .CreateTween()
                .BindNode(this)
                .SetParallel(true)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Elastic);
            tween.TweenProperty(_Item, "scale:x", scale.X, 0.25f);
            tween.TweenProperty(_Item, "scale:y", scale.Y, 0.25f).SetDelay(0.05f);
            tween.TweenProperty(_Item, "scale:z", scale.Z, 0.25f).SetDelay(0.1f);

        }
    }
}

