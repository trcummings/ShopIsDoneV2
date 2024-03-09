using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Core;
using ShopIsDone.Lighting;
using SystemGenerics = System.Collections.Generic;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Tiles
{
	public partial class Tile : StaticBody3D
	{
		public Vector3 TilemapPosition;

		public bool Enabled = true;

        #region Placement Tile Exports
        [ExportGroup("Placement Tile")]
        [Export]
        private DirEnum.Dir EditorFacingDir
        {
            get { return DirEnum.VectorToDir(PlacementFacingDir); }
            set
            {
                _EditorFacingDir = DirEnum.DirToVector(value);
                PlacementFacingDir = DirEnum.DirToVector(value);
            }
        }
        private Vector3 _EditorFacingDir = Vector3.Forward;

        public Vector3 PlacementFacingDir
        {
            get { return _PlacementFacingDir; }
            set { _PlacementFacingDir = value; }
        }
        public Vector3 _PlacementFacingDir = Vector3.Forward;
        #endregion

        [ExportGroup("")]
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

        /* Simple outwards BFS using manhattan distance */
        public Array<Tile> GetTilesInRange(int range, bool useManhattanDistance = true)
        {
            SystemGenerics.HashSet<Tile> visited = new SystemGenerics.HashSet<Tile>();
            SystemGenerics.Queue<Tile> queue = new SystemGenerics.Queue<Tile>();
            Array<Tile> tilesWithinRange = new Array<Tile>();

            // Add the initial tile to the queue and track that it was visited
            queue.Enqueue(this);
            visited.Add(this);

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue();

                // Pick which distance to use
                var distance = useManhattanDistance
                    ? TilemapPosition.ManhattanDistance(currentTile.TilemapPosition)
                    : TilemapPosition.DistanceTo(currentTile.TilemapPosition);

                // If the current candidate tile is within the distance from the
                // initial tile, add it and enqueue its neighbors
                if (distance <= range)
                {
                    tilesWithinRange.Add(currentTile);

                    foreach (var neighborPair in currentTile.FindNeighbors())
                    {
                        Tile neighbor = neighborPair.Value;
                        if (!visited.Contains(neighbor))
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }

            return tilesWithinRange;
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
