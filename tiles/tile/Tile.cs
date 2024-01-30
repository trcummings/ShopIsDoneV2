using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Lighting;

namespace ShopIsDone.Tiles
{
	public partial class Tile : StaticBody3D
	{
		public Vector3 TilemapPosition;

		public bool Enabled = true;

        [Export]
        private RayCast3D _North;

        [Export]
        private RayCast3D _South;

        [Export]
        private RayCast3D _East;

        [Export]
        private RayCast3D _West;

        [Export]
        private RayCast3D _UnitDetector;

        [Export]
        private RayCast3D _ObstacleDetector;

        [Export]
        private LightDetector _LightDetector;

        public bool HasObstacleOnTile { get { return _HasObstacleOnTile; } }
        private bool _HasObstacleOnTile;

        public LevelEntity UnitOnTile { get { return _UnitOnTile; } }
        private LevelEntity _UnitOnTile = null;

        public void Update()
        {
            UpdateUnitOnTile();
            UpdateTileObstacle();
        }

        public Dictionary<Vector3, Tile> FindNeighbors()
		{
            // Get all possible neighbors
            var n = FindNeighbor(_North);
            var e = FindNeighbor(_East);
            var w = FindNeighbor(_West);
            var s = FindNeighbor(_South);

            Dictionary<Vector3, Tile> NeighborDictionary = new Dictionary<Vector3, Tile>();

            // Set in neighbor dict
            if (n != null) NeighborDictionary[Vector3.Forward] = n;
            if (e != null) NeighborDictionary[Vector3.Right] = e;
            if (w != null) NeighborDictionary[Vector3.Left] = w;
            if (s != null) NeighborDictionary[Vector3.Back] = s;

            return NeighborDictionary;
        }

        private Tile FindNeighbor(RayCast3D ray)
        {
            // If we get any tile as neighbor, return it
            if (ray.IsColliding() && ray.GetCollider() is Tile tile)
            {
                return tile;
            }
            return null;
        }

        public Tile GetNeighborAtDir(Vector3 dir)
        {
            // Get neighbors
            var neighbors = FindNeighbors();

            if (!neighbors.ContainsKey(dir)) return null;
            return neighbors[dir];
        }

        public bool HasUnitOnTile()
        {
            return _UnitOnTile != null;
        }

        public bool IsTileAvailable()
        {
            return !(HasObstacleOnTile || HasUnitOnTile());
        }

        public bool IsLit()
        {
            return _LightDetector.IsLit();
        }

        private void UpdateUnitOnTile()
        {
            if (_UnitDetector.IsColliding() && _UnitDetector.GetCollider() is LevelEntity unit)
            {
                // If detected unit is in the arena then set it
                if (unit.IsInArena()) _UnitOnTile = unit;
                // Otherwise null it out
                else _UnitOnTile = null;
            }
            // Null out any old values
            else _UnitOnTile = null;
        }

        private void UpdateTileObstacle()
        {
            _HasObstacleOnTile = _ObstacleDetector.GetCollider() != null;
        }
    }
}
