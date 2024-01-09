using Godot;
using ShopIsDone.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopIsDone.ArenaInteractions.Selectors
{
    // This component associates our tile world map with entities that are
    // selectable by the player, and to help determine an entity's association
    // with any tiles nearby it
    public partial class ArenaInteractionSelector : Node3D
    {
        // Selectable tiles that associate the entity to where it can be
        // selected on the board
        protected List<ArenaInteractionSelectorTile> _SelectableTiles = new List<ArenaInteractionSelectorTile>();

        public override void _Ready()
        {
            base._Ready();
            // Ready selectable tiles
            _SelectableTiles = GetChildren().OfType<ArenaInteractionSelectorTile>().ToList();
            // Hide em
            foreach (var tile in _SelectableTiles) tile.Hide();
        }

        public void Init(InteractionComponent interaction)
        {
            // Register tiles with interactables and the associated tile
            SetSelectableInTiles(interaction);
        }

        public Tile GetClosestSelectableTile(Tile tile)
        {
            return _SelectableTiles
                .OrderBy(iTile => iTile.GlobalPosition.DistanceTo(tile.GlobalPosition))
                .Select(iTile => iTile.Tile)
                .FirstOrDefault();
        }

        public List<ArenaInteractionSelectorTile> GetSelectableTiles()
        {
            return _SelectableTiles;
        }

        private void SetSelectableInTiles(InteractionComponent interaction)
        {
            // Register tiles with interaction and the associated tile
            foreach (var tile in _SelectableTiles)
            {
                tile.Selectable = this;
                tile.Interaction = interaction;
            }
        }
    }
}


