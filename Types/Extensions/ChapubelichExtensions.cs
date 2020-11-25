using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ChapubelichBot.Types.Extensions
{
    internal static class ChapubelichExtensions
    {
        public static string ToMoneyFormat(this long moneySum)
        {
            return $"{moneySum:n0}";
        }

        public static IEnumerable<T> TakeTopValues<T>(this IEnumerable<T> source, int count) where T : IComparable<T>
        {
            return source.OrderByDescending(x => x).Distinct().Take(count);
        }
    }
}
