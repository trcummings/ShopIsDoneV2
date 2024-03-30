using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Microgames.FindBathroom
{
	public partial class MoveNode : Area2D
	{
        public Vector2 TilemapPosition;

		[Export]
		private RayCast2D _Up;

        [Export]
        private RayCast2D _Down;

        [Export]
        private RayCast2D _Left;

        [Export]
        private RayCast2D _Right;

        public Dictionary<Vector2, MoveNode> GetNeighbors()
        {
            var result = new Dictionary<Vector2, MoveNode>();
            var up = GetNeighbor(_Up);
            var down = GetNeighbor(_Down);
            var left = GetNeighbor(_Left);
            var right = GetNeighbor(_Right);

            if (up != null) result.Add(Vector2.Up, up);
            if (down != null) result.Add(Vector2.Down, down);
            if (left != null) result.Add(Vector2.Left, left);
            if (right != null) result.Add(Vector2.Right, right);

            return result;
        }

        private MoveNode GetNeighbor(RayCast2D ray)
        {
            if (ray.GetCollider() is MoveNode node) return node;
            return null;
        }
    }
}
