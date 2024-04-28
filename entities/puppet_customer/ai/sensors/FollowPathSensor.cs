using System;
using Godot;
using Godot.Collections;
using ShopIsDone.AI;
using ShopIsDone.Core;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using System.Linq;

namespace ShopIsDone.Entities.PuppetCustomers.AI
{
    // This sensor analyzes where the entity is in relation to the path, and
    // sets the current path index
    public partial class FollowPathSensor : Sensor
    {
        [Export]
        private Path3D _Path;

        [Inject]
        private TileManager _TileManager;

        [Export]
        private bool _IsMovingForward = true;

        [Export]
        private bool _ShouldReverseAtEnds = false;

        // State
        private Array<Tile> _AllTiles = new Array<Tile>();

        public override void Init()
        {
            base.Init();
            // Convert path into list of tiles that we will follow circularly
            _AllTiles = ConvertPath3DToTiles(_Path, 0.5f);
        }

        public override void Sense(LevelEntity entity, Dictionary<string, Variant> blackboard)
        {
            base.Sense(entity, blackboard);

            // Get the current tile the entity is on
            var currentTile = _TileManager.GetTileAtTilemapPos(entity.TilemapPosition);

            // Get the entity's tile move handler
            var moveHandler = entity.GetComponent<TileMovementHandler>();

            // Get what index we're at in the path
            int startIndex = _AllTiles.IndexOf(currentTile);

            // If we're not on the path, clear the blackboard and return early
            if (startIndex == -1 && blackboard.ContainsKey(Consts.FOLLOW_PATH_TILES))
            {
                blackboard.Remove(Consts.FOLLOW_PATH_TILES);
                return;
            }

            var availableMoves = moveHandler.GetAvailableMoves(currentTile, false).ToGodotArray();

            // Set up vars to loop over moves
            Array<Tile> movePlan = new Array<Tile>();
            int countMoves = 0;
            int currentIndex = startIndex;

            while (countMoves < moveHandler.BaseMove)
            {
                int nextIndex = currentIndex + (_IsMovingForward ? 1 : -1);

                // Handle wrapping or reversing at the ends of the path
                if (nextIndex >= _AllTiles.Count || nextIndex < 0)
                {
                    if (_ShouldReverseAtEnds)
                    {
                        _IsMovingForward = !_IsMovingForward;  // Reverse direction
                        nextIndex = currentIndex + (_IsMovingForward ? 1 : -1);
                    }
                    else
                    {
                        // Wrap around to the beginning or end of the path
                        nextIndex = _IsMovingForward ? 0 : _AllTiles.Count - 1;
                    }
                }

                // Add the tile if it's an available move
                if (availableMoves.Contains(_AllTiles[nextIndex]))
                {
                    movePlan.Add(_AllTiles[nextIndex]);
                    countMoves++;
                }
                // If we can't move there for whatever reason, break instead of
                // just continuing
                else break;

                // Update the index
                currentIndex = nextIndex;
            }

            // Set the value in the blackboard
            blackboard[Consts.FOLLOW_PATH_TILES] = movePlan;
        }

        private Array<Tile> ConvertPath3DToTiles(Path3D path, float stepSize)
        {
            var tiles = new Array<Tile>();
            var curve = path?.Curve;

            // Catch null case
            if (curve == null) return tiles;

            // Total length of the curve
            float pathLength = curve.GetBakedLength();
            Tile previousTile = null;

            for (float t = 0; t <= pathLength; t += stepSize)
            {
                // Sample the curve and add the path's global position to it to
                // get the global position of the sample point
                Vector3 point = curve.SampleBaked(t) with { Y = 0 } + path.GlobalPosition.Round();
                Vector3 nearestTilePos = _TileManager.GlobalToMap(point);

                // Check if there's a tile at this position and it's not the
                // same as the previous one
                if (_TileManager.HasTileAtTilemapPos(nearestTilePos))
                {
                    Tile currentTile = _TileManager.GetTileAtTilemapPos(nearestTilePos);
                    if (currentTile != null && currentTile != previousTile)
                    {
                        tiles.Add(currentTile);
                        previousTile = currentTile; // Update the previous tile
                    }
                }
            }

            // If the start and end are the same, remove the end
            if (tiles[0] == tiles.Last()) tiles.RemoveAt(tiles.IndexOf(tiles.Last()));

            return tiles;
        }
    }
}

