using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using Microsoft.EntityFrameworkCore;

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

        public static async Task<int> ConcurrencyChangeValueAsync(this ChapubelichdbContext dbContext, Action changeValue)
        {
            int result = 0;
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    changeValue();
                    result = await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;
                    await ex.Entries.Single().ReloadAsync();
                }
            } while (saveFailed);
            return result;
        }
    }
}
