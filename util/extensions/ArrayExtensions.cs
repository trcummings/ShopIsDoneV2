using System;
using Godot;
using Godot.Collections;

namespace Utils.Extensions
{
	public static class ArrayExtensions
	{
        public static T PickRandom<[MustBeVariant] T>(this Array<T> list)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            // Pick a random integer modulated by the number of samples
            int randIdx = (int)(GD.Randi() % list.Count);

            // Return it
            return list[randIdx];
        }
    }
}

