using Godot;
using System;
using Godot.Collections;

namespace ShopIsDone.Tiles
{
	public partial class TileManager : GridMap
	{
        [Export]
        private Node3D _Tiles;

        [Export]
        private Array<PackedScene> _TileScenes;

        // Tiles by tilemap position
        private Dictionary<Vector3, Tile> _TilesByPos = new Dictionary<Vector3, Tile>();

        public override void _Ready()
        {
            base._Ready();
            // Hide self
            Hide();
        }

        public void Init()
        {
            foreach (var cell in GetUsedCells())
            {
                var cellItem = GetCellItem(cell);
                var scene = _TileScenes[cellItem];
                // Ignore if no configuration for it
                if (scene == null) continue;
                // Otherwise, create the tile there and add it as a child
                var tileScene = scene.Instantiate<Tile>();
                _Tiles.AddChild(tileScene);
                tileScene.TilemapPosition = cell;
                tileScene.GlobalPosition = ToGlobal(MapToLocal(cell));
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
            SetCellItem(new Vector3I((int)tilePos.X, (int)tilePos.Y, (int)tilePos.Z), 1);
        }

        public void RemoveTile(Tile tile)
        {
            var tilePos = tile.TilemapPosition;
            // Remove from dictionary
            if (_TilesByPos.ContainsKey(tilePos)) _TilesByPos.Remove(tilePos);
            // Remove entry from tilemap
            SetCellItem(new Vector3I((int)tilePos.X, (int)tilePos.Y, (int)tilePos.Z), (int)InvalidCellItem);
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
    }
}
