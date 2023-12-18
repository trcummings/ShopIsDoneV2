using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Extensions
{
    public static class ListExtensions
    {
        public static T PickRandom<T>(this List<T> list)
        {
            // Pick a random integer modulated by the number of samples
            int randIdx = (int)(GD.Randi() % list.Count);

            // Return it
            return list[randIdx];
        }

        public static IEnumerable<T> Shuffle<T>(this List<T> source)
        {
            return source.OrderBy(_ => GD.Randf());
        }
    }
}

