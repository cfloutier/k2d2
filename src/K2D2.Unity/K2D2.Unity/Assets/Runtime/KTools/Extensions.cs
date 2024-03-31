using System;
using UnityEngine;


namespace KTools
{
    public static class Extensions
    {
        public static int FloorTen(this int i)
        {
            return ((int)Mathf.Floor(i / 10f)) * 10;
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
        private static IFormatProvider inv
                    = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        public static string ToStringInvariant<T>(this T obj, string format = null)
        {
            return (format == null) ? System.FormattableString.Invariant($"{obj}")
                                    : String.Format(inv, $"{{0:{format}}}", obj);
        }

        public static T Clamp<T>(T value, T min, T max)
            where T : System.IComparable<T> {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }
   
    }
}
