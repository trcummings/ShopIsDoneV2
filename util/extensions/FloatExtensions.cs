using System;

namespace ShopIsDone.Utils.Extensions
{
    public static class FloatExtensions
    {
        public static float ShiftRange(this float oldValue, float oldMin, float oldMax, float newMin, float newMax)
        {
            // Find old range
            var oldRange = oldMax - oldMin;

            // Guard for zero range
            if (oldRange == 0) return newMin;

            // Linear conversion
            var newRange = newMax - newMin;
            return ((oldValue - oldMin) * newRange / oldRange) + newMin;
        }
    }
}