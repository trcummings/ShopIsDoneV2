using System;
using Godot;

namespace ShopIsDone.Utils.Extensions
{
	public static class Vector3Extensions
	{
		public static Vector3I ToVec3i(this Vector3 vec)
		{
			return new Vector3I((int)vec.X, (int)vec.Y, (int)vec.Z);
		}

		public static float ManhattanDistance(this Vector3 source, Vector3 target)
		{
			return
				Mathf.Abs(source.X - target.X) +
				Mathf.Abs(source.Y - target.Y) +
				Mathf.Abs(source.Z - target.Z);
		}
	}
}

