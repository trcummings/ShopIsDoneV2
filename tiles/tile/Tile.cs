using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Tiles
{
	public partial class Tile : StaticBody3D
	{
		public Vector3 TilemapPosition;

		public bool Enabled = true;

		public Dictionary<Vector3, Tile> FindNeighbors()
		{
			return new Dictionary<Vector3, Tile>();
		}

        public Tile GetNeighborAtDir(Vector3 dir)
        {
            // Get neighbors
            var neighbors = FindNeighbors();

            if (!neighbors.ContainsKey(dir)) return null;
            return neighbors[dir];
        }
    }
}
