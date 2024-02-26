using System;
using Godot;
using System.Collections.Generic;
using ShopIsDone.Tiles;
using System.Linq;

namespace ShopIsDone.Utils.Pathfinding
{
	public class FurthestTileBFS
	{
        public Tile FindFurthestTile(IEnumerable<Tile> allTiles, IEnumerable<Tile> tileSet)
        {
            // Create a dictionary to track the furthest distance tiles
            Dictionary<Tile, int> maxDistances = new Dictionary<Tile, int>();

            // Initialize distances to 0 for all tiles
            foreach (var tile in allTiles) maxDistances[tile] = 0;

            // Loop over each tile the tileset and compute its distances from
            // the given tiles
            foreach (var startTile in tileSet)
            {
                Dictionary<Tile, int> distances = new Dictionary<Tile, int>();
                Queue<Tile> queue = new Queue<Tile>();

                queue.Enqueue(startTile);
                distances[startTile] = 0;

                while (queue.Count > 0)
                {
                    Tile currentTile = queue.Dequeue();
                    int currentDistance = distances[currentTile];

                    foreach (var neighbor in currentTile.FindNeighbors())
                    {
                        Tile neighborTile = neighbor.Value;
                        if (!distances.ContainsKey(neighborTile))
                        {
                            distances[neighborTile] = currentDistance + 1;
                            queue.Enqueue(neighborTile);

                            // Update the max distance for this tile
                            if (maxDistances[neighborTile] < distances[neighborTile])
                            {
                                maxDistances[neighborTile] = distances[neighborTile];
                            }
                        }
                    }
                }
            }

            // Find the tile with the maximum of minimum distances
            Tile furthestTile = null;
            int maxDistance = -1;

            foreach (var pair in maxDistances)
            {
                if (pair.Value > maxDistance)
                {
                    maxDistance = pair.Value;
                    furthestTile = pair.Key;
                }
            }

            return furthestTile;
        }

        public List<Tile> FindTilesOrderedByDistance(IEnumerable<Tile> allTiles, IEnumerable<Tile> tileSet)
        {
            Dictionary<Tile, int> distanceSums = new Dictionary<Tile, int>();

            // Initialize distances to 0 for all tiles
            foreach (var tile in allTiles) distanceSums[tile] = 0;

            foreach (var startTile in tileSet)
            {
                Dictionary<Tile, int> distances = new Dictionary<Tile, int>();
                Queue<Tile> queue = new Queue<Tile>();

                queue.Enqueue(startTile);
                distances[startTile] = 0;

                while (queue.Count > 0)
                {
                    Tile currentTile = queue.Dequeue();
                    int currentDistance = distances[currentTile];

                    foreach (var neighbor in currentTile.FindNeighbors())
                    {
                        Tile neighborTile = neighbor.Value;
                        if (!distances.ContainsKey(neighborTile))
                        {
                            distances[neighborTile] = currentDistance + 1;
                            queue.Enqueue(neighborTile);

                            // Aggregate the distance for this tile
                            if (!distanceSums.ContainsKey(neighborTile))
                            {
                                distanceSums[neighborTile] = 0;
                            }
                            distanceSums[neighborTile] += distances[neighborTile];
                        }
                    }
                }
            }

            // Sort the tiles by their aggregated distances
            var sortedTiles = distanceSums.ToList();
            sortedTiles.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            // Extract just the tiles in sorted order
            List<Tile> orderedTiles = sortedTiles.Select(pair => pair.Key).ToList();

            return orderedTiles;
        }
    }
}

