using Godot;
using ShopIsDone.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Selectables
{
    // This component associates our tile world map with entities that are
    // selectable by the player, and to help determine an entity's association
    // with any tiles nearby it
    public partial class Selectable : Node3DComponent, IUpdateOnActionComponent
    {
        // Selectable tiles that associate the entity to where it can be
        // selected on the board
        protected List<SelectableTile> _SelectableTiles = new List<SelectableTile>();

        public override void _Ready()
        {
            base._Ready();
            // Ready selectable tiles
            _SelectableTiles = GetChildren().OfType<SelectableTile>().ToList();
            // Hide em
            foreach (var tile in _SelectableTiles) tile.Hide();
        }

        public override void Init()
        {
            // Register tiles with interactables and the associated tile
            SetSelectableInTiles();
        }

        public Command UpdateOnAction()
        {
            return new ActionCommand(SetSelectableInTiles);
        }

        public Tile GetClosestSelectableTile(Tile tile)
        {
            return _SelectableTiles
                .OrderBy(iTile => iTile.GlobalPosition.DistanceTo(tile.GlobalPosition))
                .Select(iTile => iTile.Tile)
                .FirstOrDefault();
        }

        public List<SelectableTile> GetSelectableTiles()
        {
            return _SelectableTiles;
        }

        private void SetSelectableInTiles()
        {
            // Register tiles with interaction and the associated tile
            foreach (var tile in _SelectableTiles) tile.Selectable = this;
        }
    }
}


