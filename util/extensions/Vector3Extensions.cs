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
	}
}

