using Godot;
using Godot.Collections;
using ShopIsDone.AI;
using ShopIsDone.Arenas;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.DependencyInjection;
using System;
using System.Linq;
using ShopIsDone.Utils.Extensions;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
    public partial class NearestEmployeeSensor : Sensor
    {
        [Inject]
        private PlayerUnitService _PlayerUnitService;

        [Inject]
        private TileManager _TileManager;

        public override void Sense(LevelEntity entity, Dictionary<string, Variant> blackboard)
        {
            base.Sense(entity, blackboard);

            var closestUnit = _PlayerUnitService
                .GetUnits()
                .OrderBy(unit =>
                    entity.TilemapPosition.DistanceTo(unit.TilemapPosition))
                .FirstOrDefault();

            // Clear keys if we have no target
            if (closestUnit == null)
            {
                blackboard.Remove(Consts.ENTITY_TARGET);
                blackboard.Remove(Consts.TILE_TARGET);
                blackboard.Remove(Consts.PRIORITY_GOAL_TILES);
            }
            else
            {
                var unitTile = _TileManager.GetTileAtTilemapPos(closestUnit.TilemapPosition);
                blackboard[Consts.ENTITY_TARGET] = closestUnit;
                blackboard[Consts.TILE_TARGET] = unitTile;
                blackboard[Consts.PRIORITY_GOAL_TILES] = GetPrioritizedGoalTiles(closestUnit.FacingDirection, unitTile);
            }
        }

        // Get target's neighbors, sort by opposite-from-facing-direction-ness
        private Array<Tile> GetPrioritizedGoalTiles(Vector3 targetFacingDir, Tile targetTile)
        {
            return targetTile
                .FindNeighbors()
                .Select(kv => kv.Value)
                // Filter out tiles that are already occupied
                .Where(tile => !tile.HasUnitOnTile())
                .OrderByDescending(tile =>
                {
                    // Get direction the tile is "facing" relative to the unit
                    var dir = targetTile.TilemapPosition - tile.TilemapPosition;

                    // Behind is highest priority
                    if (dir == targetFacingDir) return 2;
                    // In front is lowest priority
                    else if (dir == targetFacingDir.Reflect(Vector3.Up)) return 0;
                    // Everything else is equal
                    else return 1;
                })
                .ToGodotArray();
        }
    }
}

