using Godot;
using System;

namespace ShopIsDone.Actors
{
    public partial class ActorFloorIndicator : Node3D
    {
        private Sprite3D _Sprite;
        private Node3D _Pivot;

        public override void _Ready()
        {
            _Sprite = GetNode<Sprite3D>("%Sprite");
            _Pivot = GetNode<Node3D>("%Pivot");
        }

        public void UpdateIndicator(Vector3 dir)
        {
            var lookAt = dir with { Y = 0 };
            // If velocity is zero, or the look at is the same as the pivot pos,
            // return early
            if (lookAt.IsEqualApprox(Vector3.Zero)) return;
            // Otherwise, look at
            _Pivot.LookAt(_Pivot.GlobalPosition + lookAt, Vector3.Up);
        }
    }
}
