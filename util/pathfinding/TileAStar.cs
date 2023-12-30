using System;
using System.Collections.Generic;
using System.Linq;
using ShopIsDone.Tiles;

namespace ShopIsDone.Utils.Pathfinding
{
    public interface INeighborStrategy
    {
        IEnumerable<Tile> FindValidNeighbors(Tile tile, List<Tile> allTiles);
    }

    public class SimpleNeighborStrategy : INeighborStrategy
    {
        public IEnumerable<Tile> FindValidNeighbors(Tile tile, List<Tile> allTiles)
        {
            // Use the tile's "Find Neighbors" function with no qualification
            return tile.FindNeighbors()
                .Select(kv => kv.Value)
                .Where(allTiles.Contains);
        }
    }

    // A* Search for tiles
    public class TileAStar
    {
        // Tracking for tiles
        HashSet<Tile> _ClosedSet = new HashSet<Tile>();
        HashSet<Tile> _OpenSet = new HashSet<Tile>();

        // Cost of start to this key node
        Dictionary<Tile, int> _GScore = new Dictionary<Tile, int>();
        // Cost of start to goal, passing through key node
        Dictionary<Tile, int> _FScore = new Dictionary<Tile, int>();

        // Links between tiles
        Dictionary<Tile, Tile> _NodeLinks = new Dictionary<Tile, Tile>();

        // Strategy pattern for injecting alternative neighbor finding functions
        private INeighborStrategy _NeighborStrategy;

        public TileAStar(INeighborStrategy neighborStrategy = null)
        {
            _NeighborStrategy = neighborStrategy ?? new SimpleNeighborStrategy();
        }

        public List<Tile> GetMovePath(List<Tile> availableMoves, Tile start, Tile goal)
        {
            // Initially track the start tiles
            _OpenSet.Add(start);
            _GScore[start] = 0;
            _FScore[start] = Heuristic(start, goal);

            // Loop over open set
            while (_OpenSet.Count > 0)
            {
                // Get the best next tile in the open set
                var current = GetNextBestTile();

                // If we're at the goal, return a reconstructed list of the tiles
                if (current.Equals(goal)) return Reconstruct(current);

                // Remove the current tile from the open set and add to closed set
                _OpenSet.Remove(current);
                _ClosedSet.Add(current);

                // Loop over the neighbors and process them
                var neighbors = _NeighborStrategy.FindValidNeighbors(current, availableMoves);
                foreach (var neighbor in neighbors)
                {
                    // If the neighbor is already in the closed set, ignore
                    if (_ClosedSet.Contains(neighbor)) continue;

                    // Calculate the projected G Score
                    var projectedG = GetGScore(current) + 1;

                    // Add the neighbor to the open set if it's not already
                    if (!_OpenSet.Contains(neighbor)) _OpenSet.Add(neighbor);
                    // But if the neighbor's projected G is higher than the
                    // actual G score, ignore
                    else if (projectedG >= GetGScore(neighbor)) continue;

                    // Record the neighbor's links and scores
                    _NodeLinks[neighbor] = current;
                    _GScore[neighbor] = projectedG;
                    _FScore[neighbor] = projectedG + Heuristic(neighbor, goal);
                }
            }

            // If we didn't come up with anything then just return empty
            return new List<Tile>();
        }

        private int Heuristic(Tile start, Tile goal)
        {
            var dx = goal.TilemapPosition.X - start.TilemapPosition.X;
            var dy = goal.TilemapPosition.Y - start.TilemapPosition.Y;

            return Math.Abs((int)dx) + Math.Abs((int)dy);
        }

        private int GetGScore(Tile pt)
        {
            int score = int.MaxValue;
            _GScore.TryGetValue(pt, out score);
            return score;
        }


        private int GetFScore(Tile pt)
        {
            int score = int.MaxValue;
            _GScore.TryGetValue(pt, out score);
            return score;
        }

        private Tile GetNextBestTile()
        {
            int best = int.MaxValue;
            Tile bestPt = null;
            foreach (var node in _OpenSet)
            {
                var score = GetFScore(node);
                if (score < best)
                {
                    bestPt = node;
                    best = score;
                }
            }

            return bestPt;
        }

        private List<Tile> Reconstruct(Tile current)
        {
            List<Tile> path = new List<Tile>();
            while (_NodeLinks.ContainsKey(current))
            {
                path.Add(current);
                current = _NodeLinks[current];
            }

            path.Reverse();
            return path;
        }
    }
}