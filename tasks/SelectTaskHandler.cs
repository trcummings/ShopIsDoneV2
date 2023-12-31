﻿using System;
using System.Linq;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Tasks
{
	public partial class SelectTaskHandler : Node3DComponent
    {
		[Export]
		private TaskComponent _Task;

        // Selectable tiles that associate the entity to where it can be
        // selected on the board
        protected Array<SelectTaskArea> _SelectableTiles = new Array<SelectTaskArea>();

        public override void _Ready()
        {
            base._Ready();
            // Ready selectable tiles
            _SelectableTiles = GetChildren().OfType<SelectTaskArea>().ToGodotArray();
            // Hide em
            foreach (var tile in _SelectableTiles)
            {
                tile.Hide();
                tile.Task = _Task;
            }
        }

        public Tile GetClosestSelectableTile(Tile tile)
        {
            return _SelectableTiles
                .OrderBy(iTile => iTile.GlobalPosition.DistanceTo(tile.GlobalPosition))
                .Select(iTile => iTile.Tile)
                .FirstOrDefault();
        }
    }
}

