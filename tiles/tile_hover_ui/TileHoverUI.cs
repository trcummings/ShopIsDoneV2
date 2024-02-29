using Godot;
using ShopIsDone.Game;
using System;

namespace ShopIsDone.Tiles.UI
{
    public partial class TileHoverUI : Control
    {
        // Nodes
        private Label _TileCoordinates;
        private Sprite2D _EyeIcon;

        public override void _Ready()
        {
            // Ready nodes
            _TileCoordinates = GetNode<Label>("%TileCoordinates");
            _EyeIcon = GetNode<Sprite2D>("%EyeIcon");
        }

        public void SelectTile(Tile tile)
        {
            if (GameManager.IsDebugMode())
            {
                // Set tile coordinates
                _TileCoordinates.Text = "(" + tile.TilemapPosition.X + ", " + tile.TilemapPosition.Z + ")";
            }
            else _TileCoordinates.Hide();

            // Set eye icon based on if tile is lit or not
            if (tile.IsLit()) _EyeIcon.Frame = 0;
            else _EyeIcon.Frame = 1;
        }
    }
}
