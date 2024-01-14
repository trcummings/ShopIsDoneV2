using Godot;
using Godot.Collections;
using ShopIsDone.ActionPoints;
using ShopIsDone.Actions;
using ShopIsDone.Arenas;
using ShopIsDone.Core;
using ShopIsDone.EntityStates;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.Positioning;
using System;
using System.Linq;
using ActionConsts = ShopIsDone.Actions.Consts;

namespace ShopIsDone.Entities.BehindSpirit
{
    public partial class BehindSpiritService : Node
    {
        [Export]
        public bool IsActive = false;

        [Export]
        private LevelEntity _BehindSpirit;

        [Export]
        private TileManager _TileManager;

        [Export]
        private PlayerUnitService _PlayerUnitService;

        [Export]
        private ActionService _ActionService;

        private ActionHandler _ActionHandler;
        private EntityStateHandler _StateHandler;

        public bool CanExecute()
        {
            return IsActive && GetUnitsWithExposedBacks().Count > 0;
        }

        public Command Execute()
        {
            // Make sure we have our components
            _ActionHandler = _BehindSpirit.GetComponent<ActionHandler>();
            _StateHandler = _BehindSpirit.GetComponent<EntityStateHandler>();

            // Pick target
            var target = PickTarget(GetUnitsWithExposedBacks());

            // Warp to the back tile
            _BehindSpirit.GlobalPosition = target.BackTile.GlobalPosition;
            // Face towards the target
            _BehindSpirit.FacingDirection = target.Unit.FacingDirection;
            // Get lurking strike action
            var action = _ActionHandler.GetAction(Consts.Actions.LURKING_STRIKE);

            // Execute the action
            return _ActionService.ExecuteAction(action, new Dictionary<string, Variant>()
            {
                { ActionConsts.TARGET, target.Unit },
                // Get positioning of action
                { ActionConsts.POSITIONING, (int)Positions.Behind }
            });
        }

        private partial class UnitTilePair : GodotObject
        {
            public LevelEntity Unit;
            public Tile BackTile;
            public ActionPointHandler ApHandler;
        }

        // NB: This method assumes we have at least one in the arguments
        private UnitTilePair PickTarget(Array<UnitTilePair> pairs)
        {
            // If all units have the same AP debt, then just pick randomly
            var firstDebt = pairs.First().ApHandler.ActionPointDebt;
            if (pairs.All(pair => pair.ApHandler.ActionPointDebt == firstDebt))
            {
                var newPairs = pairs.ToGodotArray();
                newPairs.Shuffle();
                return newPairs.First();
            }

            // Otherwise, sort by the highest AP debt, and pick that one
            return pairs
                .OrderByDescending(pair => pair.ApHandler.ActionPointDebt)
                .First();
        }

        // Gets an array of units tupled with the tile their back is exposed to
        private Array<UnitTilePair> GetUnitsWithExposedBacks()
        {
            var result = new Array<UnitTilePair>();

            // Get all units on the player team
            var units = _PlayerUnitService.GetActiveUnits();

            // Get their backs
            foreach (var unit in units)
            {
                // Get the tile the unit is on
                var tile = _TileManager.GetTileAtTilemapPos(unit.TilemapPosition);
                // Figure out which direction their back is in
                var oppositeDir = unit.FacingDirection.Reflect(Vector3.Up);
                var backPos = tile.TilemapPosition + oppositeDir;

                // If we don't have a tile in that position, continue the loop
                if (!_TileManager.HasTileAtTilemapPos(backPos))
                {
                    continue;
                }

                // Otherwise, get that tile, and check if it's available
                var backTile = _TileManager.GetTileAtTilemapPos(backPos);
                // If it is, add it to our array
                if (!backTile.HasObstacleOnTile && !backTile.HasUnitOnTile())
                {
                    result.Add(new UnitTilePair()
                    {
                        Unit = unit,
                        BackTile = backTile,
                        ApHandler = unit.GetComponent<ActionPointHandler>()
                    });
                }
            }

            return result;
        }
    }
}

