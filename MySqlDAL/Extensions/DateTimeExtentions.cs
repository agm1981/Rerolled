using System;

namespace Common.Extensions
{
    public static class DateTimeExtentions
    {
        public static DateTime? ToLocalTime(this DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.ToLocalTime();
        }

        /// <summary>
        /// Date must be in UTC
        /// </summary>
        /// <param name="dateTimeInUtc"></param>
        /// <returns></returns>
        public static int DateTimeToUnixTimestamp(this DateTime dateTimeInUtc)
        {
            return Convert.ToInt32((dateTimeInUtc - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }
    }
}
