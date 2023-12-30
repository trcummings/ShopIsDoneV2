using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Tiles;

namespace ShopIsDone.Utils.Pathfinding
{
    // Helper class for tracking navigation data
    public class TileNavigationData
    {
        public Tile Root = null;
        public int Distance = 0;
        public List<Tile> Neighbors = new List<Tile>();
    }

    // Strategy patterns for deciding to skip tile
    public interface IShouldSkipTileStrategy
    {
        bool ShouldSkipTile(Tile currentCandidate, Tile currentNeighbor);
    }

    // Default strategy
    public class SimpleShouldSkipTileStrategy : IShouldSkipTileStrategy
    {
        public virtual bool ShouldSkipTile(Tile _, Tile currentNeighbor)
        {
            return !currentNeighbor.Enabled;
        }
    }

    // Move generator class
    public class MoveGenerator
    {
        private IShouldSkipTileStrategy _ShouldSkipTileStrategy;

        public MoveGenerator(IShouldSkipTileStrategy skipStrategy = null)
        {
            _ShouldSkipTileStrategy = skipStrategy ?? new SimpleShouldSkipTileStrategy();
        }

        // State
        private readonly Dictionary<Vector3, TileNavigationData> _TileNavMap = new Dictionary<Vector3, TileNavigationData>();

        public List<Tile> GetAvailableMoves(Tile InitialTile, bool includeSelf = true, int moveRange = 0)
        {
            // Create list of available moves
            List<Tile> AvailableMoves = new List<Tile>();

            // Clear nav map
            _TileNavMap.Clear();

            // Get starting tile from pawn
            Tile currentCandidateTile = InitialTile;
            var ProcessQueue = new Queue<Tile>();

            // Add the initial candidate tile to the process queue
            ProcessQueue.Enqueue(currentCandidateTile);

            // Set the tilemap data and root ourselves to ourselves
            AddToTileNavMap(currentCandidateTile);
            var candidateNavData = _TileNavMap[currentCandidateTile.TilemapPosition];
            candidateNavData.Root = currentCandidateTile;
            UpdateNavData(currentCandidateTile.TilemapPosition, candidateNavData);

            // Add the initial tile to the list of available moves
            AvailableMoves.Add(currentCandidateTile);

            // Start a while loop and process the queue until it's empty
            while (ProcessQueue.Count > 0)
            {
                // Pop the first item off the list
                currentCandidateTile = ProcessQueue.Dequeue();

                // Add it to the tile nav map data (only adds if it hasn't been added yet)
                AddToTileNavMap(currentCandidateTile);

                // Get the candidate tile's data
                candidateNavData = _TileNavMap[currentCandidateTile.TilemapPosition];

                // Get the candidate tile's neighbors
                var neighbors = candidateNavData.Neighbors;

                // Loop over each neighbor and check if we should add them to the list
                foreach (var neighbor in neighbors)
                {
                    // Add neighbor to the nav data map
                    AddToTileNavMap(neighbor);

                    // Get neighbor data
                    var neighborNavData = _TileNavMap[neighbor.TilemapPosition];

                    // If we haven't been rooted, skip
                    if (!(neighborNavData.Root == null && neighbor != currentCandidateTile))
                    {
                        continue;
                    }

                    // Check if we should skip this tile for any other reason
                    if (_ShouldSkipTileStrategy.ShouldSkipTile(currentCandidateTile, neighbor))
                    {
                        continue;
                    }

                    // If we can, update the nav data for that neighbor
                    neighborNavData.Root = currentCandidateTile;
                    neighborNavData.Distance = candidateNavData.Distance + 1;
                    UpdateNavData(neighbor.TilemapPosition, neighborNavData);

                    // If the neighbor isn't too far away, add it to the list of
                    // available moves and also check it as a candidate
                    if (neighborNavData.Distance <= moveRange)
                    {
                        AvailableMoves.Add(neighbor);
                    }

                    ProcessQueue.Enqueue(neighbor);
                }
            }

            return includeSelf
                ? AvailableMoves :
                // If we shouldn't include ourselves, filter initial tile out
                AvailableMoves.Where(tile => tile != InitialTile).ToList();
        }

        private void AddToTileNavMap(Tile tile)
        {
            // Return early if we already have it in the dictionary
            if (_TileNavMap.ContainsKey(tile.TilemapPosition)) return;

            // Otherwise add it (and calculate the tile's neighbors while we're at it)
            _TileNavMap.Add(tile.TilemapPosition, new TileNavigationData()
            {
                Neighbors = tile.FindNeighbors().Values.ToList()
            });
        }

        private void UpdateNavData(Vector3 tilePosition, TileNavigationData data)
        {
            _TileNavMap.Remove(tilePosition);
            _TileNavMap.Add(tilePosition, data);
        }
    }
}