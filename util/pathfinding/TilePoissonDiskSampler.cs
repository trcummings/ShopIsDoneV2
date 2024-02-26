using System;
using Godot.Collections;
using Godot;
using ShopIsDone.Tiles;
using System.Linq;
using ShopIsDone.Utils.Extensions;
using Utils.Extensions;

namespace ShopIsDone.Utils.Pathfinding
{
    public class TilePoissonDiskSampler
    {
        private int _MinDistance;
        private int _MaxDistance;
        private Array<Tile> _SamplePoints = new Array<Tile>();
        private Array<Tile> _ValidTiles = new Array<Tile>();
        private Dictionary<Vector3, Tile> _ValidTilesDict = new Dictionary<Vector3, Tile>();
        private RandomNumberGenerator _RNG = new RandomNumberGenerator();

        public TilePoissonDiskSampler(int minDistance, int maxDistance)
        {
            _MinDistance = minDistance;
            _MaxDistance = maxDistance;
        }

        public Array<Tile> Generate(Array<Tile> validTiles)
        {
            // Clear sample points
            _SamplePoints.Clear();
            // Set valid points
            _ValidTiles = validTiles;
            // Create valid tiles dict for quick lookup
            _ValidTilesDict = validTiles
                .Aggregate(new Dictionary<Vector3, Tile>(), (acc, tile) =>
                {
                    acc.Add(tile.TilemapPosition, tile);
                    return acc;
                });

            // Return if no valid tiles are given
            if (_ValidTiles.Count == 0) return _SamplePoints;
            // Return if min distance is greater than max distance
            if (_MinDistance > _MaxDistance) return _SamplePoints;

            // Start with a random point from the valid points
            int initialPointIndex = _RNG.RandiRange(0, _ValidTiles.Count - 1);
            Tile initialPoint = _ValidTiles[initialPointIndex];
            _ValidTiles.RemoveAt(initialPointIndex);

            // Initialize a list of tiles to process with our initial chosen point
            Array<Tile> processList = new Array<Tile> { initialPoint };
            _SamplePoints.Add(initialPoint);

            // Keep looping through the list until it's empty
            while (processList.Count > 0)
            {
                // Grab a random tile to process against
                int pointIndex = _RNG.RandiRange(0, processList.Count - 1);
                Tile candidateTile = processList[pointIndex];

                // Get positions in range to select from
                var positionsInRange = candidateTile
                    // Get all tiles in range around the candidate tile
                    .GetTilesInRange(_MaxDistance)
                    // Filter down to only valid tiles
                    .Where(t => _ValidTilesDict.ContainsKey(t.TilemapPosition))
                    // Make sure the points are a minimum distance away from all
                    // other sample points
                    .Where(IsMinDistanceAwayFromSamples)
                    // Map to positions, then shuffle them
                    .Select(t => t.TilemapPosition)
                    .ToList()
                    .Shuffle();

                // If the list is empty, remove the candidate tile
                if (positionsInRange.Count() == 0)
                {
                    processList.RemoveAt(pointIndex);
                }
                // Otherwise, add the first one to the process list and then to
                // the sample points
                else
                {
                    var newTile = GetTileAtPosition(positionsInRange.First());
                    processList.Add(newTile);
                    _SamplePoints.Add(newTile);
                }
            }

            return _SamplePoints;
        }

        private bool IsMinDistanceAwayFromSamples(Tile tile)
        {
            return _SamplePoints.All(sampleTile =>
                sampleTile.TilemapPosition.ManhattanDistance(tile.TilemapPosition) >= _MinDistance
            );
        }

        private Tile GetTileAtPosition(Vector3 tilePos)
        {
            if (_ValidTilesDict.TryGetValue(tilePos, out Tile tile)) return tile;
            return null;
        }
    }
}