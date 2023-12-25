using System;
using Godot;

namespace ShopIsDone.Utils.Positioning
{
    public enum Positions
    {
        Null = -1,
        Behind = 0,
        Facing = 1,
        AtSide = 2
    }

    public static class Positioning
    {
        public static Positions GetPositioning(Vector3 aggressorDir, Vector3 targetDir)
        {
            // If the aggressor's direction is the same as the target's, that means
            // the aggressor is directly behind the target
            if (aggressorDir == targetDir) return Positions.Behind;

            // If their direction is the opposite as the target's, that means
            // they're facing eachother
            if (aggressorDir == targetDir.Reflect(Vector3.Up)) return Positions.Facing;

            // Otherwise, it's at their sides, so return a medium value
            return Positions.AtSide;
        }
    }
}

