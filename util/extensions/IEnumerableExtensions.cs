using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GodotCollections = Godot.Collections;

namespace ShopIsDone.Utils.Extensions
{
    public static class IEnumerableExtensions
    {
        // WITH PREVIOUS AND NEXT
        public static IEnumerable<(T Previous, T Current, T Next)> WithPreviousAndNext<T>(
            this IEnumerable<T> source,
            T firstPrevious = default,
            T lastNext = default
        )
        {
            // Error against a null value
            if (source == null) throw new ArgumentNullException("source");
            return WithPreviousAndNextImpl(source, firstPrevious, lastNext);
        }

        static IEnumerable<(T Previous, T Current, T Next)> WithPreviousAndNextImpl<T>(
            this IEnumerable<T> source,
            T firstPrevious = default,
            T lastNext = default
        )
        {
            (T Previous, T Current, bool HasPrevious) queue = (default, firstPrevious, false);

            foreach (var item in source)
            {
                if (queue.HasPrevious)
                {
                    yield return (queue.Previous, queue.Current, item);
                }
                queue = (queue.Current, item, true);
            }

            if (queue.HasPrevious)
            {
                yield return (queue.Previous, queue.Current, lastNext);
            }
        }

        // WITH PREVIOUS
        public static IEnumerable<(T Previous, T Current)> WithPrevious<T>(
            this IEnumerable<T> source,
            T firstPrevious = default
        )
        {
            // Error against a null value
            if (source == null) throw new ArgumentNullException("source");
            return WithPreviousImpl(source, firstPrevious);
        }

        static IEnumerable<(T Previous, T Current)> WithPreviousImpl<T>(
            this IEnumerable<T> source,
            T firstPrevious = default
        )
        {
            (T Previous, T Current, bool HasPrevious) queue = (default, firstPrevious, false);

            foreach (var item in source)
            {
                if (queue.HasPrevious)
                {
                    yield return (queue.Previous, queue.Current);
                }
                queue = (queue.Current, item, true);
            }

            if (queue.HasPrevious)
            {
                yield return (queue.Previous, queue.Current);
            }
        }

        // NON CONSECUTIVE VALUES
        public static IEnumerable<T> NonConsecutive<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return NonConsecutiveImpl(source);
        }

        static IEnumerable<T> NonConsecutiveImpl<T>(this IEnumerable<T> source)
        {
            bool isFirst = true;
            T last = default(T);
            foreach (var item in source)
            {
                if (isFirst || !object.Equals(item, last))
                {
                    yield return item;
                    last = item;
                    isFirst = false;
                }
            }
        }

        // CIRCULAR SELECTION
        public static T SelectCircular<T>(this IEnumerable<T> source, int idx, int dir)
        {
            // Throw if source is null
            if (source == null) throw new ArgumentNullException("source");
            return SelectCircularImpl(source, idx, dir);
        }

        private static T SelectCircularImpl<T>(this IEnumerable<T> source, int idx, int dir)
        {
            // Get the sign of the direction value
            var dirSign = Mathf.Sign(dir);

            // If the direction value is 0, throw
            if (dirSign == 0) throw new ArgumentException("'dir' must not be 0!");

            // Select circularly
            T value = source.ElementAtOrDefault(idx + dirSign);
            if (value == null) value = (dirSign == 1 ? source.First() : source.Last());
            return value;
        }

        // RANGE
        public static IEnumerable<int> Range(int start, int stop, int step = 1)
        {
            if (step == 0)
                throw new ArgumentException(nameof(step));

            while (step > 0 && start < stop || step < 0 && start > stop)
            {
                yield return start;
                start += step;
            }
        }

        public static IEnumerable<int> Range(int stop) => Range(0, stop, 1);


        // To Godot Array
        private static GodotCollections.Array<T> ToGodotArrayImpl<[MustBeVariant] T>(this IEnumerable<T> source)
        {
            var arr = new GodotCollections.Array<T>();
            foreach (var entry in source) arr.Add(entry);
            return arr;
        }

        public static GodotCollections.Array<T> ToGodotArray<[MustBeVariant] T>(this IEnumerable<T> source)
        {
            // Throw if source is null
            if (source == null) throw new ArgumentNullException("source");
            return ToGodotArrayImpl(source);
        }
    }
}