using System.Collections.Generic;

namespace MySqlDAL.Extensions
{
    public static class GenericExtensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return (list as IList<T>).Contains(source);
        }
    }
}