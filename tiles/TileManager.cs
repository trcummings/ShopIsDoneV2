using Godot;
using System;
using Godot.Collections;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Core;

namespace ShopIsDone.Tiles
{
	public partial class TileManager : Node3D, IService
    {
        [Export]
        private GridMap _ArenaTilemap;

        [Export]
        private Array<PackedScene> _TileScenes;

        // Tiles by tilemap position
        private Dictionary<Vector3, Tile> _TilesByPos = new Dictionary<Vector3, Tile>();

        public override void _Ready()
        {
            base._Ready();
            // Hide tilemap
            _ArenaTilemap.Hide();
        }

        public void Init()
        {
            foreach (var cell in _ArenaTilemap.GetUsedCells())
            {
                var cellItem = _ArenaTilemap.GetCellItem(cell);
                var scene = _TileScenes[cellItem];
                // Ignore if no configuration for it
                if (scene == null) continue;
                // Otherwise, create the tile there and add it as a child
                var tileScene = scene.Instantiate<Tile>();
                AddChild(tileScene);
                tileScene.TilemapPosition = cell;
                var cellPosition = _ArenaTilemap.ToGlobal(_ArenaTilemap.MapToLocal(cell));
                tileScene.GlobalPosition = cellPosition;
                // Set cell in dictionary
                SetTile(tileScene);
            }
        }

        public void SetTile(Tile tile)
        {
            var tilePos = tile.TilemapPosition;
            _TilesByPos.Add(tilePos, tile);

            // Create entry in tilemap so we can convert between tilemap
            // position and world position
            _ArenaTilemap.SetCellItem(new Vector3I((int)tilePos.X, (int)tilePos.Y, (int)tilePos.Z), 1);
        }

        public void RemoveTile(Tile tile)
        {
            var tilePos = tile.TilemapPosition;
            // Remove from dictionary
            if (_TilesByPos.ContainsKey(tilePos)) _TilesByPos.Remove(tilePos);
            // Remove entry from tilemap
            _ArenaTilemap.SetCellItem(new Vector3I((int)tilePos.X, (int)tilePos.Y, (int)tilePos.Z), (int)GridMap.InvalidCellItem);
        }

        public bool HasTileAtTilemapPos(Vector3 tilemapPosition)
        {
            return _TilesByPos.ContainsKey(tilemapPosition);
        }

        public Tile GetTileAtTilemapPos(Vector3 tilemapPosition)
        {
            if (_TilesByPos.TryGetValue(tilemapPosition, out Tile tile))
            {
                return tile;
            }
            return null;
        }

        public bool IsTileOccupied(Tile tile)
        {
            return false;
        }

        public bool CanPassThroughTile(LevelEntity entity, Tile tile)
        {
            return true;
        }
    }
}
