using System;
using Godot;

namespace ShopIsDone.Tiles
{
    public static class DirEnum
	{
        public enum Dir
        {
            Forward = 0,
            Back = 1,
            Left = 2,
            Right = 3
        }

        public static Dir VectorToDir(Vector3 dir)
        {
            // Compare against the current facing direction
            if (dir == Vector3.Forward) return Dir.Forward;
            if (dir == Vector3.Back) return Dir.Back;
            if (dir == Vector3.Left) return Dir.Left;
            if (dir == Vector3.Right) return Dir.Right;
            return Dir.Forward;
        }

        public static Vector3 DirToVector(Dir dir)
        {
            switch (dir)
            {
                case Dir.Forward:
                    {
                        return Vector3.Forward;
                    }
                case Dir.Back:
                    {
                        return Vector3.Back;
                    }
                case Dir.Left:
                    {
                        return Vector3.Left;
                    }
                case Dir.Right:
                    {
                        return Vector3.Right;
                    }
            }
            return Vector3.Forward;
        }
    }
}

