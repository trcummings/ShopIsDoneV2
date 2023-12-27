using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Tiles
{
	public partial class Tile : Node3D
	{
		public Vector3 TilemapPosition;

		public bool Enabled = true;

		public Dictionary<Vector3, Tile> FindNeighbors()
		{
			return new Dictionary<Vector3, Tile>();
		}
	}
}
