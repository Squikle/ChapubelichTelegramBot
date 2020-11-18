namespace ChapubelichBot.Types.Extensions
{
    internal static class ChapubelichExtensions
    {
        public static string ToMoneyFormat(this int moneySum)
        {
            return $"{moneySum:n0}";
        }
        public static string ToMoneyFormat(this long moneySum)
        {
            return $"{moneySum:n0}";
        }
    }
}
