using System;

namespace MySqlDAL.Extensions
{
    public static class DateTimeExtentions
    {
        public static DateTime? ToLocalTime(this DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.ToLocalTime();
        }
    }
}
