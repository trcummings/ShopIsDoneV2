using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Utils.Extensions
{
	public static class Node3DExtensions
	{
        public static void SafeLookAtFromPosition(this Node3D spatial, Vector3 target, Vector3 position)
        {
            // Just return if it's the same position
            if (position == target) return;

            // Grab the normalized difference between the two
            var vZ = (position - target).Normalized();

            // Find an up vector that we can rotate around
            var up = Vector3.Zero;
            foreach (var entry in new Array<Vector3>() { Vector3.Right, Vector3.Up, Vector3.Back })
            {
                var vX = entry.Cross(vZ).Normalized();
                if (vX.Length() != 0) up = entry;
            }

            // Look at the target
            if (up != Vector3.Zero) spatial.LookAtFromPosition(position, target, up);
        }
    }
}

