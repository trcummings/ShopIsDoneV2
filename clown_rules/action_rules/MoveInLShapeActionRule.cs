using Godot;
using ShopIsDone.Actions;
using ShopIsDone.Tiles;
using ShopIsDone.Utils.Extensions;
using System;
using Godot.Collections;
using System.Linq;

namespace ShopIsDone.ClownRules.ActionRules
{
    public partial class MoveInLShapeActionRule : ClownActionRule
    {
        private Array<Tile> _MovePath = new Array<Tile>();

        public override bool BrokeRule(ArenaAction action, Dictionary<string, Variant> message)
        {
            // If it's not a move sub action, ignore it
            if (action is not MoveSubAction moveSubAction) return false;

            // If it's the last sub move, evaluate
            if (moveSubAction.IsLastMove()) return MovedInLShape(_MovePath);

            // If this is our first sub action, add the initial tile to the path
            if (_MovePath.Count == 0) _MovePath.Add(moveSubAction.NextMove.InitialTile);

            // Update the move path with the next tile
            _MovePath.Add(moveSubAction.NextMove.FinalTile);

            // Continue on
            return false;
        }

        public override void ResetRule()
        {
            _MovePath.Clear();
        }

        protected bool MovedInLShape(Array<Tile> movePath)
        {
            // We moved in a L-shape if if the move vectors are two unbroken chunks
            // of the same vector and nothing else)
            var numChunks = movePath
                .WithPrevious(null)
                // Skip first where prev value is null
                .SkipWhile((record) => record.Previous == null)
                // Compare the values to get a diffed list of all movement
                .Select((record) => record.Previous.TilemapPosition - record.Current.TilemapPosition)
                // Make list nonconsecutive
                .NonConsecutive()
                // Get count
                .Count();

            // If num chunks is equal to 2, we moved in an L shape
            return numChunks == 2;
        }
    }
}