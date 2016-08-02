using System;

namespace MySqlDAL.Extensions
{
    public enum TimeSpanType : long
    {
        Quarterly = TimeSpanExtensions.QuarterlyTicks,
        Monthly = TimeSpanExtensions.MonthlyTicks,
        BiWeekly = TimeSpanExtensions.BiWeeklyTicks,
        Weekly = TimeSpanExtensions.WeeklyTicks,
        Daily = TimeSpanExtensions.DailyTicks,
        Yearly = TimeSpanExtensions.YearlyTicks
    }

    public static class TimeSpanExtensions
    {
        public const long DailyTicks = 0; //mark made me do it.
        public const long WeeklyTicks = TimeSpan.TicksPerDay * 7;
        public const long BiWeeklyTicks = WeeklyTicks * 2;
        public const long MonthlyTicks = WeeklyTicks * 4;
        public const long QuarterlyTicks = MonthlyTicks * 3;
        public const long YearlyTicks = MonthlyTicks * 4;

        public static bool IsYearly(this TimeSpan value)
        {
            return value.Ticks >= YearlyTicks;
        }
        public static bool IsQuarterly(this TimeSpan value)
        {
            return value.Ticks >= QuarterlyTicks && value.Ticks < YearlyTicks;
        }

        public static bool IsMonthly(this TimeSpan value)
        {
            return value.Ticks >= MonthlyTicks && value.Ticks < QuarterlyTicks;
        }

        public static bool IsBiWeekly(this TimeSpan value)
        {
            return value.Ticks >= BiWeeklyTicks && value.Ticks < MonthlyTicks;
        }

        public static bool IsWeekly(this TimeSpan value)
        {
            return value.Ticks >= WeeklyTicks && value.Ticks < BiWeeklyTicks;
        }

        public static bool IsDaily(this TimeSpan value)
        {
            return value.Ticks < WeeklyTicks;
        }

        public static TimeSpanType ConvertToType(this TimeSpan value)
        {

            if (value.IsDaily())
            {
                return TimeSpanType.Daily;
            }

            if (value.IsWeekly())
            {
                return TimeSpanType.Weekly;
            }

            if (value.IsBiWeekly())
            {
                return TimeSpanType.BiWeekly;
            }

            if (value.IsMonthly())
            {
                return TimeSpanType.Monthly;
            }

            if (value.IsQuarterly())
            {
                return TimeSpanType.Quarterly;
            }
            if (value.IsYearly())
            {
                return TimeSpanType.Yearly;
            }


            throw new Exception(string.Format("Invalid Timepan Value : {0}", value.Ticks));
        }
        

        public static string ConvertFrequencyToString(long value)
        {
            string result;
            switch (value)
            {
                case YearlyTicks:
                    result = "Yearly";
                    break;
                case QuarterlyTicks:
                    result = "Quarterly";
                    break;
                case MonthlyTicks:
                    result = "Monthly";
                    break;
                case BiWeeklyTicks:
                    result = "BiWeekly";
                    break;
                case WeeklyTicks:
                    result = "Weekly";
                    break;
                default:
                    result = "Daily";
                    break;
            }
            return result;
        }

    }
}
